using AdministratorPanel.Modules.LogCollector.Abstractions;
using AdministratorPanel.Modules.LogCollector.Models;
using Renci.SshNet;

namespace AdministratorPanel.Infrastructure.LogCollector.Services;

public sealed class SshServerDiscoveryService : IServerDiscoveryService
{
    public async Task<IReadOnlyCollection<DiscoveredServerDto>> DiscoverAsync(
    string prefix,
    int from,
    int to,
    string sshUserName,
    string password,
    CancellationToken cancellationToken = default)
    {
        var result = new List<DiscoveredServerDto>();

        var tasks = new List<Task>();

        for (int i = 1; i <= 254; i++)
        {
            var ip = $"10.10.130.{i}";

            tasks.Add(Task.Run(() =>
            {
                try
                {
                    using var client = new SshClient(ip, 22, sshUserName, password);

                    client.ConnectionInfo.Timeout = TimeSpan.FromMilliseconds(500);
                    client.Connect();

                    if (!client.IsConnected)
                        return;

                    var cmd = client.CreateCommand("hostname");
                    var hostname = cmd.Execute().Trim();

                    lock (result)
                    {
                        result.Add(new DiscoveredServerDto
                        {
                            DisplayName = string.IsNullOrWhiteSpace(hostname) ? ip : hostname,
                            IpAdress = ip
                        });
                    }

                    client.Disconnect();
                }
                catch
                {
                    // игнорируем
                }

            }, cancellationToken));
        }

        await Task.WhenAll(tasks);

        return result;
    }
}