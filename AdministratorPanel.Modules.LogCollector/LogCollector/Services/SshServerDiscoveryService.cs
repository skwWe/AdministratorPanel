using AdministratorPanel.Modules.LogCollector.Abstractions;
using AdministratorPanel.Modules.LogCollector.Models;
using Renci.SshNet;
using System.Collections.Concurrent;

namespace AdministratorPanel.Modules.LogCollector.Services
{
    public sealed class SshServerDiscoveryService : IServerDiscoveryService
    {
        private const int SshPort = 22;
        private const int MaxParallelConnections = 20;

        public async Task<IReadOnlyCollection<DiscoveredServerDto>> DiscoverAsync(
            string networkPrefix,
            int from,
            int to,
            string sshUserName,
            string password,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(networkPrefix))
            {
                networkPrefix = "10.10.130";
            }

            if (string.IsNullOrWhiteSpace(sshUserName))
            {
                throw new ArgumentException("SSH-пользователь не указан.", nameof(sshUserName));
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException("SSH-пароль не указан.", nameof(password));
            }

            var result = new ConcurrentBag<DiscoveredServerDto>();
            using var semaphore = new SemaphoreSlim(MaxParallelConnections);

            var tasks = Enumerable
                .Range(from, to - from + 1)
                .Select(async number =>
                {
                    await semaphore.WaitAsync(cancellationToken);

                    try
                    {
                        var ip = $"{networkPrefix}.{number}";

                        var discoveredServer = await TryDiscoverServerAsync(
                            ip,
                            sshUserName,
                            password,
                            cancellationToken);

                        if (discoveredServer is not null)
                        {
                            result.Add(discoveredServer);
                        }
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                })
                .ToArray();

            await Task.WhenAll(tasks);

            return result
                .OrderBy(x => x.DisplayName)
                .ToArray();
        }

        private static Task<DiscoveredServerDto?> TryDiscoverServerAsync(
            string ipAddress,
            string sshUserName,
            string password,
            CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    using var client = new SshClient(ipAddress, SshPort, sshUserName, password);

                    client.ConnectionInfo.Timeout = TimeSpan.FromSeconds(3);

                    client.Connect();

                    if (!client.IsConnected)
                    {
                        return null;
                    }

                    using var command = client.CreateCommand("hostname -f || hostname");
                    var hostName = command.Execute()?.Trim();

                    client.Disconnect();

                    if (string.IsNullOrWhiteSpace(hostName))
                    {
                        hostName = ipAddress;
                    }

                    return new DiscoveredServerDto
                    {
                        DisplayName = hostName,
                        IpAddress = ipAddress
                    };
                }
                catch
                {
                    return null;
                }
            }, cancellationToken);
        }
    }
}