using AdministratorPanel.Modules.ServerManagement.Models.Operations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdministratorPanel.Modules.ServerManagement.Abstractions
{
    public interface IServerServiceControlService
    {
        Task<ServerOperationResultDto> StopAllDockerContainersAsync(
            string serverName,
            string ipAddress,
            string sshUserName,
            string password,
            CancellationToken cancellationToken = default);

        Task<ServerOperationResultDto> RestartAllDockerContainersAsync(
            string serverName,
            string ipAddress,
            string sshUserName,
            string password,
            CancellationToken cancellationToken = default);

        Task<ServerOperationResultDto> RestartDockerServiceAsync(
            string serverName,
            string ipAddress,
            string sshUserName,
            string password,
            CancellationToken cancellationToken = default);
    }
}
