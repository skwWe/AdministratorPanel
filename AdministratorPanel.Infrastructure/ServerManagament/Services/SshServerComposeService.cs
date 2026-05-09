using AdministratorPanel.Modules.ServerManagement.Abstractions;
using AdministratorPanel.Modules.ServerManagement.Models.Operations;
using Renci.SshNet;
using System.Text;

namespace AdministratorPanel.Infrastructure.ServerManagement.Services
{
    public sealed class SshServerComposeService : IServerComposeService
    {
        private const string ComposeDirectory = "/digdes/docker_data";

        public async Task<ServerOperationResultDto> StopServicesAsync(
            string serverName, string ipAddress, string sshUserName, string password, CancellationToken cancellationToken = default)
        {
            return await ExecuteAsync(serverName, ipAddress, sshUserName, password,
                "Остановка Docker Compose-сервисов", BuildStopCommand(), cancellationToken);
        }

        public async Task<ServerOperationResultDto> RestartServicesAsync(
            string serverName, string ipAddress, string sshUserName, string password, CancellationToken cancellationToken = default)
        {
            return await ExecuteAsync(serverName, ipAddress, sshUserName, password,
                "Перезапуск Docker Compose-сервисов", BuildRestartCommand(), cancellationToken);
        }

        private static string BuildStopCommand() =>
            $"/gmn/shell_scripts/docker/docker_compose_down.sh {EscapeBashArgument(ComposeDirectory)}";

        private static string BuildRestartCommand() =>
            $"/gmn/shell_scripts/docker/docker_compose_restart.sh {EscapeBashArgument(ComposeDirectory)}";

        private static async Task<ServerOperationResultDto> ExecuteAsync(
            string serverName, string ipAddress, string sshUserName, string password,
            string operationName, string commandText, CancellationToken cancellationToken)
        {
            return await Task.Run(() =>
            {
                var output = new StringBuilder();
                var error = new StringBuilder();
                try
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    output.AppendLine($"Операция: {operationName}");
                    output.AppendLine($"Сервер: {serverName}");
                    output.AppendLine($"IP: {ipAddress}");
                    output.AppendLine($"Время: {DateTime.Now:yyyy-MM-dd HH:mm:ss}\n");

                    using var client = new SshClient(ipAddress, 22, sshUserName, password);
                    client.ConnectionInfo.Timeout = TimeSpan.FromSeconds(10);
                    client.Connect();
                    if (!client.IsConnected)
                        return new ServerOperationResultDto
                        {
                            Status = ServerOperationStatus.Failed,
                            ServerName = serverName,
                            IpAddress = ipAddress,
                            OperationName = operationName,
                            Output = output.ToString(),
                            Error = "SSH-подключение не установлено."
                        };

                    output.AppendLine("> Выполнение удалённого скрипта\n");
                    using var command = client.CreateCommand(commandText);
                    var commandOutput = command.Execute();
                    var commandError = command.Error ?? "";
                    var exitCode = command.ExitStatus ?? -1;

                    if (!string.IsNullOrWhiteSpace(commandOutput)) output.AppendLine(commandOutput);
                    if (!string.IsNullOrWhiteSpace(commandError)) error.AppendLine(commandError);
                    output.AppendLine($"\nExitCode: {exitCode}");
                    client.Disconnect();

                    return new ServerOperationResultDto
                    {
                        Status = exitCode == 0 ? ServerOperationStatus.Success : ServerOperationStatus.Failed,
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

        private static string EscapeBashArgument(string value) => "'" + value.Replace("'", "'\"'\"'") + "'";
    }
}