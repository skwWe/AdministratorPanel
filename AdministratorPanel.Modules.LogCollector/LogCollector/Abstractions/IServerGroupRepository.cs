using AdministratorPanel.Modules.LogCollector.Entities;

namespace AdministratorPanel.Modules.LogCollector.Abstractions
{
    public interface IServerGroupRepository
    {
        IReadOnlyCollection<ServerGroup> GetAll();

        ServerGroup? GetByName(string groupName);

        RemoteServer? GetServerById(Guid serverId);

        RemoteServer AddServer(
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