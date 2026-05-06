using AdministratorPanel.Modules.ServerManagement.Models.Operations;
using AdministratorPanel.Modules.ServerManagement.Models.ShutdownPlans;

namespace AdministratorPanel.Modules.ServerManagement.Abstractions
{
    public interface IServerShutdownService
    {
        Task<ServerOperationResultDto> SafeShutdownAsync(
            string groupName,
            string serverName,
            string ipAddress,
            string sshUserName,
            string password,
            ShutdownGroupRuleDto rule,
            CancellationToken cancellationToken = default);
    }
}