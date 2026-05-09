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
            string groupName, string serverName, string ipAddress, string sshUserName, string password,
            ShutdownGroupRuleDto rule, CancellationToken cancellationToken = default)
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
                    output.AppendLine($"Время: {DateTime.Now:yyyy-MM-dd HH:mm:ss}\n");

                    using var client = new SshClient(ipAddress, 22, sshUserName, password);
                    client.ConnectionInfo.Timeout = TimeSpan.FromSeconds(10);
                    client.Connect();

                    if (!client.IsConnected)
                        return Fail(serverName, ipAddress, operationName, "SSH-подключение не установлено.", output.ToString());

                    // Информация о сервере
                    Execute(client, "hostname && date && uptime", output, error);

                    // Остановка Docker Compose, если требуется
                    if (rule.HasDockerCompose)
                    {
                        string composeCommand = $"/gmn/shell_scripts/docker/docker_compose_down.sh {EscapeBashArgument(rule.ComposeDirectory)}";
                        var composeResult = Execute(client, composeCommand, output, error);
                        if (composeResult.ExitCode != 0)
                        {
                            client.Disconnect();
                            return Fail(serverName, ipAddress, operationName,
                                "Ошибка при остановке Docker Compose-сервисов.", output.ToString(), error.ToString());
                        }
                    }
                    else
                    {
                        output.AppendLine("Для данной группы Docker Compose не используется. Этап остановки контейнеров пропущен.\n");
                    }

                    // Выключение сервера
                    var shutdownResult = Execute(client, "shutdown now", output, error);
                    if (shutdownResult.ExitCode != 0)
                    {
                        output.AppendLine("Команда shutdown now завершилась с ошибкой. Пробую через sudo.");
                        var sudoShutdown = Execute(client, $"printf '%s\\n' {EscapeBashArgument(password)} | sudo -S -p '' shutdown now", output, error);
                        if (sudoShutdown.ExitCode != 0)
                        {
                            client.Disconnect();
                            return Fail(serverName, ipAddress, operationName,
                                "Не удалось выполнить shutdown now даже через sudo.", output.ToString(), error.ToString());
                        }
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

        private static CommandResult Execute(SshClient client, string commandText, StringBuilder output, StringBuilder error)
        {
            output.AppendLine($"> {commandText}");
            using var command = client.CreateCommand(commandText);
            var commandOutput = command.Execute();
            var commandError = command.Error ?? "";
            var exitCode = command.ExitStatus ?? -1;

            if (!string.IsNullOrWhiteSpace(commandOutput)) output.AppendLine(commandOutput);
            if (!string.IsNullOrWhiteSpace(commandError)) error.AppendLine(commandError);
            output.AppendLine($"ExitCode: {exitCode}\n");
            return new CommandResult { ExitCode = exitCode, Output = commandOutput ?? "", Error = commandError };
        }

        private static ServerOperationResultDto Fail(string serverName, string ipAddress, string operationName,
            string message, string output, string error = "") => new()
            {
                Status = ServerOperationStatus.Failed,
                ServerName = serverName,
                IpAddress = ipAddress,
                OperationName = operationName,
                Output = output,
                Error = string.IsNullOrWhiteSpace(error) ? message : message + Environment.NewLine + error
            };

        private static string EscapeBashArgument(string value) => "'" + value.Replace("'", "'\"'\"'") + "'";
        private sealed class CommandResult { public int ExitCode { get; init; } public string Output { get; init; } = ""; public string Error { get; init; } = ""; }
    }
}