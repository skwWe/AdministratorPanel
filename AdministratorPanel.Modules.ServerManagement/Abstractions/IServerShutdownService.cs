using AdministratorPanel.Modules.ServerManagement.Models.Operations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdministratorPanel.Modules.ServerManagement.Abstractions
{
    public interface IServerShutdownService
    {
        Task<ServerOperationResultDto> SafeShutdownAsync(
            string serverName,
            string ipAddress,
            string sshUserName,
            string password,
            CancellationToken cancellationToken = default);
    }
}
