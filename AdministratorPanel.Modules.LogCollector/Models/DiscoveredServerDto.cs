namespace AdministratorPanel.Modules.LogCollector.Models
{
    public sealed class DiscoveredServerDto
    {
        public string DisplayName { get; init; } = string.Empty;

        public string IpAddress { get; init; } = string.Empty;
    }
}