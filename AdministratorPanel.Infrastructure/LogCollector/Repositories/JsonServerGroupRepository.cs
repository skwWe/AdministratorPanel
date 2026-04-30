using AdministratorPanel.Modules.LogCollector.Abstractions;
using AdministratorPanel.Modules.LogCollector.Data;
using AdministratorPanel.Modules.LogCollector.Entities;
using System.Text.Json;

namespace AdministratorPanel.Infrastructure.LogCollector.Repositories
{
    public sealed class JsonServerGroupRepository : IServerGroupRepository
    {
        private readonly string _filePath;
        private readonly List<ServerGroup> _groups;

        public JsonServerGroupRepository()
        {
            _filePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "AdministratorPanel",
                "server-groups.json");

            _groups = LoadGroups();
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
            SaveGroups();

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

            SaveGroups();

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
                SaveGroups();

                return true;
            }

            return false;
        }

        private List<ServerGroup> LoadGroups()
        {
            if (!File.Exists(_filePath))
            {
                var defaultGroups = DefaultServerGroups.Create().ToList();
                SaveGroups(defaultGroups);
                return defaultGroups;
            }

            var json = File.ReadAllText(_filePath);

            var groups = JsonSerializer.Deserialize<List<ServerGroup>>(json, GetJsonOptions());

            return groups ?? DefaultServerGroups.Create().ToList();
        }

        private void SaveGroups()
        {
            SaveGroups(_groups);
        }

        private void SaveGroups(List<ServerGroup> groups)
        {
            var directory = Path.GetDirectoryName(_filePath);

            if (!string.IsNullOrWhiteSpace(directory))
                Directory.CreateDirectory(directory);

            var json = JsonSerializer.Serialize(groups, GetJsonOptions());

            File.WriteAllText(_filePath, json);
        }

        private static JsonSerializerOptions GetJsonOptions()
        {
            return new JsonSerializerOptions
            {
                WriteIndented = true
            };
        }
    }
}