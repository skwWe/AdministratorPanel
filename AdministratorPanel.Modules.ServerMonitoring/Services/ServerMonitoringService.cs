using System.Text.Json;
using AdministratorPanel.Modules.ServerMonitoring.Abstractions;
using AdministratorPanel.Modules.ServerMonitoring.Models;
using Renci.SshNet;

namespace AdministratorPanel.Modules.ServerMonitoring.Services
{
    public sealed class ServerMonitoringService
        : IServerMonitoringService
    {
        private const string MonitoringScriptPath =
            "bash /gmn/shell_scripts/docker/monitor.sh";

        public async Task<ServerMonitoringInfoDto>
            GetMonitoringInfoAsync(
                string serverName,
                string ipAddress,
                string sshUserName,
                string password,
                CancellationToken cancellationToken = default)
        {
            return await Task.Run(() =>
            {
                try
                {
                    using var client =
                        new SshClient(
                            ipAddress,
                            22,
                            sshUserName,
                            password);

                    client.ConnectionInfo.Timeout =
                        TimeSpan.FromSeconds(5);

                    client.Connect();

                    if (!client.IsConnected)
                    {
                        return Offline(
                            serverName,
                            ipAddress,
                            "SSH connection failed.");
                    }

                    using var command =
                        client.CreateCommand(
                            MonitoringScriptPath);

                    string json = command.Execute();

                    if (string.IsNullOrWhiteSpace(json))
                    {
                        return Offline(
                            serverName,
                            ipAddress,
                            "Monitoring script returned empty result.");
                    }

                    var result =
                        JsonSerializer.Deserialize<
                            MonitoringScriptResult>(json);

                    client.Disconnect();

                    if (result is null)
                    {
                        return Offline(
                            serverName,
                            ipAddress,
                            "Failed to parse monitoring JSON.");
                    }

                    return new ServerMonitoringInfoDto
                    {
                        ServerName = serverName,
                        IpAddress = ipAddress,
                        Status = ServerMonitoringStatus.Online,
                        Uptime = result.Uptime,
                        CpuUsage = result.Cpu,
                        MemoryUsage = result.Memory,
                        DiskUsage = result.Disk,
                        LoadAverage = result.Load,
                        RunningContainers = result.Containers,
                        DockerAvailable = result.Docker
                    };
                }
                catch (Exception ex)
                {
                    return Offline(
                        serverName,
                        ipAddress,
                        ex.Message);
                }
            }, cancellationToken);
        }

        private static ServerMonitoringInfoDto Offline(
            string serverName,
            string ipAddress,
            string error)
        {
            return new ServerMonitoringInfoDto
            {
                ServerName = serverName,
                IpAddress = ipAddress,
                Status = ServerMonitoringStatus.Offline,
                Error = error
            };
        }
    }
}