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
                return Fail(-1, $"Группа серверов не найдена: {groupName}");

            var servers = group.Servers.Where(x => x.IsEnabled).OrderBy(x => x.SortOrder).ToArray();
            if (servers.Length == 0)
                return Fail(-2, "В выбранной группе нет активных серверов.");

            var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            var baseLogDir = Path.Combine(AppContext.BaseDirectory, "logs", groupName.ToLowerInvariant(), timestamp);
            Directory.CreateDirectory(baseLogDir);

            var output = new StringBuilder();
            var errors = new StringBuilder();
            int successCount = 0, errorCount = 0;

            Report(progress, output, false, "==============================================");
            Report(progress, output, false, "Сбор Docker-логов через bash-скрипт");
            Report(progress, output, false, $"Группа: {groupName}");
            Report(progress, output, false, $"Серверов: {servers.Length}");
            Report(progress, output, false, $"Папка логов: {baseLogDir}");
            Report(progress, output, false, "==============================================");

            foreach (var server in servers)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var serverDir = Path.Combine(baseLogDir, server.IpAddress);
                Directory.CreateDirectory(serverDir);

                Report(progress, output, false, "");
                Report(progress, output, false, $"--- {server.DisplayName} / {server.IpAddress} ---");

                try
                {
                    using var client = new SshClient(server.IpAddress, 22, sshUserName, password ?? string.Empty);
                    client.ConnectionInfo.Timeout = TimeSpan.FromSeconds(10);
                    client.Connect();
                    if (!client.IsConnected)
                        throw new InvalidOperationException("SSH-подключение не установлено.");

                    const string composeDir = "/digdes/docker_data";
                    string command = $"/gmn/shell_scripts/docker/collect_logs.sh {EscapeBashArgument(composeDir)}";
                    var result = RunCommandWithSudo(client, command, password);

                    File.WriteAllText(Path.Combine(serverDir, "all.log"), result.Output);
                    if (!string.IsNullOrWhiteSpace(result.Error))
                        File.WriteAllText(Path.Combine(serverDir, "SCRIPT_ERROR.log"), result.Error);

                    if (result.ExitCode == 0)
                    {
                        successCount++;
                        Report(progress, output, false, $"Готово: {server.DisplayName}");
                    }
                    else
                    {
                        errorCount++;
                        Report(progress, errors, true, $"Ошибка скрипта на {server.DisplayName}: код {result.ExitCode}");
                    }
                    client.Disconnect();
                }
                catch (Exception ex)
                {
                    errorCount++;
                    File.WriteAllText(Path.Combine(serverDir, "SSH_ERROR.txt"), ex.ToString());
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
                ExecutedScriptPath = "bash-script",
                StandardOutput = output.ToString(),
                StandardError = errors.ToString()
            };
        }, cancellationToken);
    }

    private static CommandResult RunCommandWithSudo(SshClient client, string command, string? sudoPassword)
    {
        var result = RunCommand(client, command);
        if (result.ExitCode == 0)
            return result;

        var combined = result.Output + result.Error;
        if (!combined.Contains("permission denied", StringComparison.OrdinalIgnoreCase) &&
            !combined.Contains("отказано в доступе", StringComparison.OrdinalIgnoreCase))
            return result;

        if (string.IsNullOrWhiteSpace(sudoPassword))
            return result;

        var sudoCommand = $"printf '%s\\n' {EscapeBashArgument(sudoPassword)} | sudo -S -p '' {command}";
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

    private static string EscapeBashArgument(string value) => "'" + value.Replace("'", "'\"'\"'") + "'";
    private static void Report(IProgress<LogScriptProgressMessage>? progress, StringBuilder builder, bool isError, string text)
    {
        builder.AppendLine(text);
        progress?.Report(new LogScriptProgressMessage { IsError = isError, Text = text });
    }
    private static LogScriptExecutionResult Fail(int exitCode, string error) => new()
    {
        IsSuccess = false,
        ExitCode = exitCode,
        ExecutedScriptPath = "SSH.NET",
        StandardError = error
    };
    private static string MapGroupTypeToName(LogCollectionGroupType groupType) => groupType switch
    {
        LogCollectionGroupType.App => "App",
        LogCollectionGroupType.Convert => "Convert",
        LogCollectionGroupType.Sync => "Sync",
        LogCollectionGroupType.Web => "Web",
        _ => throw new ArgumentOutOfRangeException(nameof(groupType), groupType, null)
    };
    private sealed class CommandResult { public int ExitCode { get; init; } public string Output { get; init; } = ""; public string Error { get; init; } = ""; }
}