using AdministratorPanel.Modules.LogCollector.Abstractions;
using AdministratorPanel.Modules.LogCollector.Enums;
using AdministratorPanel.Modules.LogCollector.Models;
using Renci.SshNet;
using System.Text;

namespace AdministratorPanel.Infrastructure.LogCollector.Services;

public sealed class SshNetLogScriptRunner : ILogScriptRunner
{
    private readonly IServerGroupRepository _serverGroupRepository;

    public SshNetLogScriptRunner(IServerGroupRepository serverGroupRepository)
    {
        _serverGroupRepository = serverGroupRepository;
    }

    public async Task<LogScriptExecutionResult> RunAsync(
        LogCollectionGroupType groupType,
        string sshUserName,
        string? password,
        IProgress<LogScriptProgressMessage>? progress = null,
        CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            var groupName = MapGroupTypeToName(groupType);
            var group = _serverGroupRepository.GetByName(groupName);

            if (group is null)
            {
                return Fail(-1, $"Группа серверов не найдена: {groupName}");
            }

            var servers = group.Servers
                .Where(x => x.IsEnabled)
                .OrderBy(x => x.SortOrder)
                .ToArray();

            if (servers.Length == 0)
            {
                return Fail(-2, "В выбранной группе нет активных серверов.");
            }

            var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            var baseLogDir = Path.Combine(
                AppContext.BaseDirectory,
                "logs",
                groupName.ToLowerInvariant(),
                timestamp);

            Directory.CreateDirectory(baseLogDir);

            var output = new StringBuilder();
            var errors = new StringBuilder();

            Report(progress, output, false, "==============================================");
            Report(progress, output, false, "Сбор Docker-логов через SSH.NET");
            Report(progress, output, false, $"Группа: {groupName}");
            Report(progress, output, false, $"Серверов: {servers.Length}");
            Report(progress, output, false, $"Папка логов: {baseLogDir}");
            Report(progress, output, false, "==============================================");

            var successCount = 0;
            var errorCount = 0;

            foreach (var server in servers)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var serverDir = Path.Combine(baseLogDir, server.IpAddress);
                Directory.CreateDirectory(serverDir);

                Report(progress, output, false, "");
                Report(progress, output, false, $"--- {server.DisplayName} / {server.IpAddress} ---");

                try
                {
                    using var client = new SshClient(
                        server.IpAddress,
                        22,
                        sshUserName,
                        password ?? string.Empty);

                    client.ConnectionInfo.Timeout = TimeSpan.FromSeconds(10);
                    client.Connect();

                    if (!client.IsConnected)
                    {
                        throw new InvalidOperationException("SSH-подключение не установлено.");
                    }

                    var info = RunCommand(client, "hostname && date && id");
                    File.WriteAllText(Path.Combine(serverDir, "INFO.txt"), info.Output + info.Error);

                    var dockerPs = RunDockerCommand(
                        client,
                        "docker ps --format '{{.Names}}'",
                        password);

                    File.WriteAllText(
                        Path.Combine(serverDir, "DEBUG.txt"),
                        $"[docker ps exit code]\n{dockerPs.ExitCode}\n\n[docker ps output]\n{dockerPs.Output}\n\n[docker ps error]\n{dockerPs.Error}");

                    if (dockerPs.ExitCode != 0)
                    {
                        File.WriteAllText(
                            Path.Combine(serverDir, "DOCKER_ERROR.txt"),
                            dockerPs.Error + Environment.NewLine + dockerPs.Output);

                        Report(progress, errors, true, $"Docker недоступен на {server.DisplayName}");
                        errorCount++;
                        client.Disconnect();
                        continue;
                    }

                    var containers = dockerPs.Output
                        .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(x => x.Trim())
                        .Where(x => !string.IsNullOrWhiteSpace(x))
                        .ToArray();

                    if (containers.Length == 0)
                    {
                        File.WriteAllText(
                            Path.Combine(serverDir, "NO_CONTAINERS.txt"),
                            "Нет запущенных контейнеров.");

                        Report(progress, output, false, $"Нет запущенных контейнеров на {server.DisplayName}");
                        client.Disconnect();
                        continue;
                    }

                    var allLogs = new StringBuilder();

                    foreach (var container in containers)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        Report(progress, output, false, $"Сбор логов контейнера: {container}");

                        allLogs.AppendLine($"========== {container} ==========");

                        var logsResult = RunDockerCommand(
                            client,
                            $"docker logs {EscapeBashArgument(container)}",
                            password);

                        allLogs.AppendLine(logsResult.Output);

                        if (!string.IsNullOrWhiteSpace(logsResult.Error))
                        {
                            allLogs.AppendLine("[stderr]");
                            allLogs.AppendLine(logsResult.Error);
                        }

                        allLogs.AppendLine();
                    }

                    File.WriteAllText(Path.Combine(serverDir, "all.log"), allLogs.ToString());

                    successCount++;
                    Report(progress, output, false, $"Готово: {server.DisplayName}");

                    client.Disconnect();
                }
                catch (Exception ex)
                {
                    errorCount++;

                    File.WriteAllText(
                        Path.Combine(serverDir, "SSH_ERROR.txt"),
                        ex.ToString());

                    Report(progress, errors, true, $"Ошибка на {server.DisplayName}: {ex.Message}");
                }
            }

            Report(progress, output, false, "");
            Report(progress, output, false, "==============================================");
            Report(progress, output, false, "ГОТОВО");
            Report(progress, output, false, $"Успешно: {successCount}");
            Report(progress, output, false, $"Ошибок: {errorCount}");
            Report(progress, output, false, $"Логи: {baseLogDir}");
            Report(progress, output, false, "==============================================");

            return new LogScriptExecutionResult
            {
                IsSuccess = errorCount == 0,
                ExitCode = errorCount == 0 ? 0 : 1,
                ExecutedScriptPath = "SSH.NET",
                StandardOutput = output.ToString(),
                StandardError = errors.ToString()
            };
        }, cancellationToken);
    }

    private static CommandResult RunDockerCommand(
        SshClient client,
        string command,
        string? sudoPassword)
    {
        var result = RunCommand(client, command);

        if (result.ExitCode == 0)
            return result;

        var combined = result.Output + result.Error;

        if (!combined.Contains("permission denied", StringComparison.OrdinalIgnoreCase))
            return result;

        if (string.IsNullOrWhiteSpace(sudoPassword))
            return result;

        var sudoCommand =
            $"printf '%s\\n' {EscapeBashArgument(sudoPassword)} | sudo -S -p '' {command}";

        return RunCommand(client, sudoCommand);
    }

    private static CommandResult RunCommand(SshClient client, string commandText)
    {
        using var command = client.CreateCommand(commandText);

        var output = command.Execute();

        return new CommandResult
        {
            ExitCode = command.ExitStatus ?? -1,
            Output = output ?? string.Empty,
            Error = command.Error ?? string.Empty
        };
    }

    private static string EscapeBashArgument(string value)
    {
        return "'" + value.Replace("'", "'\"'\"'") + "'";
    }

    private static void Report(
        IProgress<LogScriptProgressMessage>? progress,
        StringBuilder builder,
        bool isError,
        string text)
    {
        builder.AppendLine(text);

        progress?.Report(new LogScriptProgressMessage
        {
            IsError = isError,
            Text = text
        });
    }

    private static LogScriptExecutionResult Fail(int exitCode, string error)
    {
        return new LogScriptExecutionResult
        {
            IsSuccess = false,
            ExitCode = exitCode,
            ExecutedScriptPath = "SSH.NET",
            StandardError = error
        };
    }

    private static string MapGroupTypeToName(LogCollectionGroupType groupType)
    {
        return groupType switch
        {
            LogCollectionGroupType.App => "App",
            LogCollectionGroupType.Convert => "Convert",
            LogCollectionGroupType.Sync => "Sync",
            LogCollectionGroupType.Web => "Web",
            _ => throw new ArgumentOutOfRangeException(nameof(groupType), groupType, null)
        };
    }

    private sealed class CommandResult
    {
        public int ExitCode { get; init; }

        public string Output { get; init; } = string.Empty;

        public string Error { get; init; } = string.Empty;
    }
}