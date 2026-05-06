using AdministratorPanel.Modules.ServerManagement.Models.Operations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdministratorPanel.Modules.ServerManagement.Abstractions
{
    public interface IServerComposeService
    {
        Task<ServerOperationResultDto> StopServicesAsync(
            string serverName,
            string ipAddress,
            string sshUserName,
            string password,
            CancellationToken cancellationToken = default);

        Task<ServerOperationResultDto> RestartServicesAsync(
            string serverName,
            string ipAddress,
            string sshUserName,
            string password,
            CancellationToken cancellationToken = default);
    }
}
