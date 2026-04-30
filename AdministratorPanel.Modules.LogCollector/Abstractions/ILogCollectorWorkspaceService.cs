using AdministratorPanel.Modules.LogCollector.Models;

namespace AdministratorPanel.Modules.LogCollector.Abstractions
{
    public interface ILogCollectorWorkspaceService
    {
        IReadOnlyCollection<ServerGroupDto> GetServerGroups();

        RemoteServerDto AddServer(
            Guid groupId,
            string displayName,
            string ipAddress,
            bool isEnabled);

        bool UpdateServer(
            Guid serverId,
            string displayName,
            string ipAddress,
            bool isEnabled);

        bool DeleteServer(Guid serverId);
    }
}