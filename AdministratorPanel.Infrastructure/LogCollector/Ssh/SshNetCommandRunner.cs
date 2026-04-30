using AdministratorPanel.Modules.LogCollector.Abstractions;
using AdministratorPanel.Modules.LogCollector.Models;
using Renci.SshNet;

namespace AdministratorPanel.Infrastructure.LogCollector.Ssh
{
    public sealed class SshNetCommandRunner : ISshCommandRunner
    {
        public async Task<SshCommandResult> RunAsync(
            string host,
            string username,
            string password,
            string command,
            CancellationToken cancellationToken = default)
        {
            return await Task.Run(() =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                using var client = new SshClient(host, username, password);

                client.Connect();

                if (!client.IsConnected)
                {
                    throw new InvalidOperationException("Не удалось подключиться к серверу по SSH.");
                }

                using var sshCommand = client.CreateCommand(command);

                var output = sshCommand.Execute();

                var result = new SshCommandResult
                {
                    ExitCode = sshCommand.ExitStatus ?? -1,
                    Output = output ?? string.Empty,
                    Error = sshCommand.Error ?? string.Empty
                };

                client.Disconnect();

                return result;
            }, cancellationToken);
        }
    }
}