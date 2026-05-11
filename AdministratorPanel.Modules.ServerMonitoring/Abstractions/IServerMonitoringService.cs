using AdministratorPanel.Modules.ServerMonitoring.Models;

namespace AdministratorPanel.Modules.ServerMonitoring.Abstractions
{
    public interface IServerMonitoringService
    {
        Task<ServerMonitoringInfoDto> GetMonitoringInfoAsync(
            string serverName,
            string ipAddress,
            string sshUserName,
            string password,
            CancellationToken cancellationToken = default);
    }
}