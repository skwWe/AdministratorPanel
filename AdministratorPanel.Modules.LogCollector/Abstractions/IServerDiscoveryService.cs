using AdministratorPanel.Modules.LogCollector.Models;

public interface IServerDiscoveryService
{
    Task<IReadOnlyCollection<DiscoveredServerDto>> DiscoverAsync(
        string prefix,
        int from,
        int to,
        string sshUserName,
        string password,
        CancellationToken cancellationToken = default);
}