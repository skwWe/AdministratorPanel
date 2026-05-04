using AdministratorPanel.Modules.LogCollector.Abstractions;
using AdministratorPanel.Modules.LogCollector.Entities;
using AdministratorPanel.Modules.LogCollector.Models;

namespace AdministratorPanel.Modules.LogCollector.Services
{
    public sealed class LogCollectorWorkspaceService : ILogCollectorWorkspaceService
    {
        private readonly IServerGroupRepository _serverGroupRepository;

        public LogCollectorWorkspaceService(IServerGroupRepository serverGroupRepository)
        {
            _serverGroupRepository = serverGroupRepository;
        }

        public IReadOnlyCollection<ServerGroupDto> GetServerGroups()
        {
            return _serverGroupRepository.GetAll()
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.Name)
                .Select(MapGroup)
                .ToArray();
        }

        public RemoteServerDto AddServer(
            Guid groupId,
            string displayName,
            string ipAddress,
            bool isEnabled)
        {
            var server = _serverGroupRepository.AddServer(
                groupId,
                displayName,
                ipAddress,
                isEnabled);

            return MapServer(server);
        }

        public bool UpdateServer(
            Guid serverId,
            string displayName,
            string ipAddress,
            bool isEnabled)
        {
            return _serverGroupRepository.UpdateServer(
                serverId,
                displayName,
                ipAddress,
                isEnabled);
        }

        public bool DeleteServer(Guid serverId)
        {
            return _serverGroupRepository.DeleteServer(serverId);
        }

        private static ServerGroupDto MapGroup(ServerGroup group)
        {
            return new ServerGroupDto
            {
                Id = group.Id,
                Name = group.Name,
                Description = group.Description,
                SortOrder = group.SortOrder,
                Servers = group.Servers
                    .OrderBy(s => s.SortOrder)
                    .ThenBy(s => s.DisplayName)
                    .Select(MapServer)
                    .ToArray()
            };
        }

        private static RemoteServerDto MapServer(RemoteServer server)
        {
            return new RemoteServerDto
            {
                Id = server.Id,
                IpAddress = server.IpAddress,
                DisplayName = server.DisplayName,
                IsEnabled = server.IsEnabled,
                SortOrder = server.SortOrder
            };
        }
    }
}