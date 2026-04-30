using AdministratorPanel.Modules.LogCollector.Abstractions;
using AdministratorPanel.Modules.LogCollector.Data;
using AdministratorPanel.Modules.LogCollector.Entities;

namespace AdministratorPanel.Infrastructure.LogCollector.Repositories
{
    public sealed class DefaultServerGroupRepository : IServerGroupRepository
    {
        private readonly List<ServerGroup> _groups;

        public DefaultServerGroupRepository()
        {
            _groups = DefaultServerGroups.Create().ToList();
        }

        public IReadOnlyCollection<ServerGroup> GetAll()
        {
            return _groups
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.Name)
                .ToArray();
        }

        public ServerGroup? GetByName(string groupName)
        {
            if (string.IsNullOrWhiteSpace(groupName))
                return null;

            return _groups.FirstOrDefault(x =>
                string.Equals(x.Name, groupName, StringComparison.OrdinalIgnoreCase));
        }

        public RemoteServer? GetServerById(Guid serverId)
        {
            return _groups
                .SelectMany(x => x.Servers)
                .FirstOrDefault(x => x.Id == serverId);
        }

        public RemoteServer AddServer(
            Guid groupId,
            string displayName,
            string ipAddress,
            bool isEnabled)
        {
            var group = _groups.FirstOrDefault(x => x.Id == groupId);

            if (group is null)
                throw new InvalidOperationException("Группа серверов не найдена.");

            var server = new RemoteServer
            {
                DisplayName = displayName.Trim(),
                IpAddress = ipAddress.Trim(),
                IsEnabled = isEnabled,
                ServerGroupId = group.Id,
                SortOrder = group.Servers.Count + 1
            };

            group.Servers.Add(server);

            return server;
        }

        public bool UpdateServer(
            Guid serverId,
            string displayName,
            string ipAddress,
            bool isEnabled)
        {
            var server = GetServerById(serverId);

            if (server is null)
                return false;

            server.DisplayName = displayName.Trim();
            server.IpAddress = ipAddress.Trim();
            server.IsEnabled = isEnabled;

            return true;
        }

        public bool DeleteServer(Guid serverId)
        {
            foreach (var group in _groups)
            {
                var server = group.Servers.FirstOrDefault(x => x.Id == serverId);

                if (server is null)
                    continue;

                group.Servers.Remove(server);
                return true;
            }

            return false;
        }
    }
}