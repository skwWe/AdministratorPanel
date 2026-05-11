namespace AdministratorPanel.Modules.ServerMonitoring.Models
{
    public sealed class ServerMonitoringInfoDto
    {
        public string ServerName { get; init; } = string.Empty;

        public string IpAddress { get; init; } = string.Empty;

        public ServerMonitoringStatus Status { get; init; }

        public string Uptime { get; init; } = string.Empty;

        public string CpuUsage { get; init; } = string.Empty;

        public string MemoryUsage { get; init; } = string.Empty;

        public string DiskUsage { get; init; } = string.Empty;

        public string LoadAverage { get; init; } = string.Empty;

        public int RunningContainers { get; init; }

        public bool DockerAvailable { get; init; }

        public string Error { get; init; } = string.Empty;
    }
}