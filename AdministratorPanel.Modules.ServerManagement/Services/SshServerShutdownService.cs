using AdministratorPanel.Modules.ServerManagement.Abstractions;
using AdministratorPanel.Modules.ServerManagement.Models.Operations;
using AdministratorPanel.Modules.ServerManagement.Models.ShutdownPlans;
using Renci.SshNet;
using System.Text;

namespace AdministratorPanel.Infrastructure.ServerManagement.Services
{
    public sealed class SshServerShutdownService : IServerShutdownService
    {
        public async Task<ServerOperationResultDto> SafeShutdownAsync(
            string groupName,
            string serverName,
            string ipAddress,
            string sshUserName,
            string password,
            ShutdownGroupRuleDto rule,
            CancellationToken cancellationToken = default)
        {
            return await Task.Run(() =>
            {
                const string operationName = "Безопасное отключение сервера";

                var output = new StringBuilder();
                var error = new StringBuilder();

                try
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    output.AppendLine($"Операция: {operationName}");
                    output.AppendLine($"Группа: {groupName}");
                    output.AppendLine($"Сервер: {serverName}");
                    output.AppendLine($"IP: {ipAddress}");
                    output.AppendLine($"Время: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                    output.AppendLine();

                    using var client = new SshClient(ipAddress, 22, sshUserName, password);
                    client.ConnectionInfo.Timeout = TimeSpan.FromSeconds(10);
                    client.Connect();

                    if (!client.IsConnected)
                    {
                        return Fail(
                            serverName,
                            ipAddress,
                            operationName,
                            "SSH-подключение не установлено.",
                            output.ToString());
                    }

                    Execute(client, "hostname && date && uptime", output, error);

                    if (rule.HasDockerCompose)
                    {
                        var directoryCheck = Execute(
                            client,
                            $"test -d {EscapeBashArgument(rule.ComposeDirectory)}",
                            output,
                            error);

                        if (directoryCheck.ExitCode != 0)
                        {
                            client.Disconnect();

                            return Fail(
                                serverName,
                                ipAddress,
                                operationName,
                                $"Папка Docker Compose не найдена: {rule.ComposeDirectory}",
                                output.ToString(),
                                error.ToString());
                        }

                        var stopComposeCommand = BuildStopAllComposeFilesCommand(rule.ComposeDirectory);

                        var composeStopResult = Execute(
                            client,
                            stopComposeCommand,
                            output,
                            error);

                        if (composeStopResult.ExitCode != 0)
                        {
                            client.Disconnect();

                            return Fail(
                                serverName,
                                ipAddress,
                                operationName,
                                "Не удалось корректно остановить Docker Compose-сервисы.",
                                output.ToString(),
                                error.ToString());
                        }

                        Execute(client, "docker ps --format '{{.Names}}'", output, error);
                    }
                    else
                    {
                        output.AppendLine("Для данной группы Docker Compose не используется. Этап остановки контейнеров пропущен.");
                        output.AppendLine();
                    }

                    var shutdownResult = Execute(client, "shutdown now", output, error);

                    if (shutdownResult.ExitCode != 0)
                    {
                        output.AppendLine("Команда shutdown now завершилась с ошибкой. Пробую выполнить через sudo.");
                        output.AppendLine();

                        Execute(
                            client,
                            $"printf '%s\\n' {EscapeBashArgument(password)} | sudo -S -p '' shutdown now",
                            output,
                            error);
                    }

                    client.Disconnect();

                    return new ServerOperationResultDto
                    {
                        Status = ServerOperationStatus.Success,
                        ServerName = serverName,
                        IpAddress = ipAddress,
                        OperationName = operationName,
                        Output = output.ToString(),
                        Error = error.ToString()
                    };
                }
                catch (Exception ex)
                {
                    return new ServerOperationResultDto
                    {
                        Status = ServerOperationStatus.Failed,
                        ServerName = serverName,
                        IpAddress = ipAddress,
                        OperationName = operationName,
                        Output = output.ToString(),
                        Error = ex.ToString()
                    };
                }
            }, cancellationToken);
        }

        private static string BuildStopAllComposeFilesCommand(string composeDirectory)
        {
            return "bash -lc " + EscapeBashArgument($@"
                set -euo pipefail

                TARGET_DIR={composeDirectory}

                if [ ! -d ""$TARGET_DIR"" ]; then
                    echo ""Ошибка: '$TARGET_DIR' не директория.""
                    exit 1
                fi

                cd ""$TARGET_DIR""

                shopt -s nullglob
                files=( *.yml *.yaml )

                if [ ${{#files[@]}} -eq 0 ]; then
                    echo ""Файлы .yml/.yaml не найдены в $TARGET_DIR""
                    exit 0
                fi

                for file in ""${{files[@]}}""; do
                    echo
                    echo ""=== Останавливаю $file ===""

                    if ! docker-compose -f ""$file"" down; then
                        echo ""Предупреждение: $file не удалось остановить.""
                        exit 2
                    fi
                done

                echo
                echo ""Все compose-файлы в '$TARGET_DIR' обработаны.""
                ");
        }

        private static CommandResult Execute(
            SshClient client,
            string commandText,
            StringBuilder output,
            StringBuilder error)
        {
            output.AppendLine($"> {commandText}");

            using var command = client.CreateCommand(commandText);

            var commandOutput = command.Execute();
            var commandError = command.Error ?? string.Empty;
            var exitCode = command.ExitStatus ?? -1;

            if (!string.IsNullOrWhiteSpace(commandOutput))
                output.AppendLine(commandOutput);

            if (!string.IsNullOrWhiteSpace(commandError))
                error.AppendLine(commandError);

            output.AppendLine($"ExitCode: {exitCode}");
            output.AppendLine();

            return new CommandResult
            {
                ExitCode = exitCode,
                Output = commandOutput ?? string.Empty,
                Error = commandError
            };
        }

        private static ServerOperationResultDto Fail(
            string serverName,
            string ipAddress,
            string operationName,
            string message,
            string output,
            string error = "")
        {
            return new ServerOperationResultDto
            {
                Status = ServerOperationStatus.Failed,
                ServerName = serverName,
                IpAddress = ipAddress,
                OperationName = operationName,
                Output = output,
                Error = string.IsNullOrWhiteSpace(error)
                    ? message
                    : message + Environment.NewLine + error
            };
        }

        private static string EscapeBashArgument(string value)
        {
            return "'" + value.Replace("'", "'\"'\"'") + "'";
        }

        private sealed class CommandResult
        {
            public int ExitCode { get; init; }

            public string Output { get; init; } = string.Empty;

            public string Error { get; init; } = string.Empty;
        }
    }
}