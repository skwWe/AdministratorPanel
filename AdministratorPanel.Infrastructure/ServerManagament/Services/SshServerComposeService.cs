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
            string serverName,
            string ipAddress,
            string sshUserName,
            string password,
            CancellationToken cancellationToken = default)
        {
            return await ExecuteAsync(
                serverName,
                ipAddress,
                sshUserName,
                password,
                "Остановка Docker Compose-сервисов",
                BuildStopCommand(),
                cancellationToken);
        }

        public async Task<ServerOperationResultDto> RestartServicesAsync(
            string serverName,
            string ipAddress,
            string sshUserName,
            string password,
            CancellationToken cancellationToken = default)
        {
            return await ExecuteAsync(
                serverName,
                ipAddress,
                sshUserName,
                password,
                "Перезапуск Docker Compose-сервисов",
                BuildRestartCommand(),
                cancellationToken);
        }

        private static string BuildStopCommand()
        {
            return "bash -lc " + EscapeBashArgument($@"
set -euo pipefail

TARGET_DIR={ComposeDirectory}

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
        echo ""Предупреждение: $file не удалось остановить (пропускаю).""
    fi
done

echo
echo ""Все compose-файлы в '$TARGET_DIR' обработаны.""
");
        }

        private static string BuildRestartCommand()
        {
            return "bash -lc " + EscapeBashArgument($@"
set -euo pipefail

TARGET_DIR={ComposeDirectory}

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
        echo ""Предупреждение: $file не удалось остановить (пропускаю).""
    fi
done

echo
echo ""=== Выполняю docker volume prune ===""
docker volume prune -f

for file in ""${{files[@]}}""; do
    echo
    echo ""=== Запускаю $file ===""

    docker-compose -f ""$file"" up -d
done

echo
echo ""Все compose-файлы в '$TARGET_DIR' обработаны.""
");
        }

        private static async Task<ServerOperationResultDto> ExecuteAsync(
            string serverName,
            string ipAddress,
            string sshUserName,
            string password,
            string operationName,
            string commandText,
            CancellationToken cancellationToken)
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
                    output.AppendLine($"Время: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                    output.AppendLine();

                    using var client = new SshClient(ipAddress, 22, sshUserName, password);
                    client.ConnectionInfo.Timeout = TimeSpan.FromSeconds(10);
                    client.Connect();

                    if (!client.IsConnected)
                    {
                        return new ServerOperationResultDto
                        {
                            Status = ServerOperationStatus.Failed,
                            ServerName = serverName,
                            IpAddress = ipAddress,
                            OperationName = operationName,
                            Output = output.ToString(),
                            Error = "SSH-подключение не установлено."
                        };
                    }

                    output.AppendLine("> Выполнение удалённого скрипта");
                    output.AppendLine();

                    using var command = client.CreateCommand(commandText);

                    var commandOutput = command.Execute();
                    var commandError = command.Error ?? string.Empty;
                    var exitCode = command.ExitStatus ?? -1;

                    if (!string.IsNullOrWhiteSpace(commandOutput))
                    {
                        output.AppendLine(commandOutput);
                    }

                    if (!string.IsNullOrWhiteSpace(commandError))
                    {
                        error.AppendLine(commandError);
                    }

                    output.AppendLine();
                    output.AppendLine($"ExitCode: {exitCode}");

                    client.Disconnect();

                    return new ServerOperationResultDto
                    {
                        Status = exitCode == 0
                            ? ServerOperationStatus.Success
                            : ServerOperationStatus.Failed,
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

        private static string EscapeBashArgument(string value)
        {
            return "'" + value.Replace("'", "'\"'\"'") + "'";
        }
    }
}