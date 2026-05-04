using AdministratorPanel.Modules.LogCollector.Models;

namespace AdministratorPanel.Modules.LogCollector.Abstractions
{
    public interface IServerDiscoveryService
    {
        Task<IReadOnlyCollection<DiscoveredServerDto>> DiscoverAsync(
            string networkPrefix,
            int from,
            int to,
            string sshUserName,
            string password,
            CancellationToken cancellationToken = default);
    }
}