using System.Text.Json.Serialization;

namespace AdministratorPanel.Modules.ServerMonitoring.Models
{
    public sealed class MonitoringScriptResult
    {
        [JsonPropertyName("hostname")]
        public string Hostname { get; init; } = string.Empty;

        [JsonPropertyName("kernel")]
        public string Kernel { get; init; } = string.Empty;

        [JsonPropertyName("uptime")]
        public string Uptime { get; init; } = string.Empty;

        [JsonPropertyName("cpu")]
        public string Cpu { get; init; } = string.Empty;

        [JsonPropertyName("memory")]
        public string Memory { get; init; } = string.Empty;

        [JsonPropertyName("disk")]
        public string Disk { get; init; } = string.Empty;

        [JsonPropertyName("load")]
        public string Load { get; init; } = string.Empty;

        [JsonPropertyName("docker")]
        public bool Docker { get; init; }

        [JsonPropertyName("containers")]
        public int Containers { get; init; }
    }
}