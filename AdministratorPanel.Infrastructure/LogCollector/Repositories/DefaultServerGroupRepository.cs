using AdministratorPanel.Modules.LogCollector.Abstractions;
using AdministratorPanel.Modules.LogCollector.Data;
using AdministratorPanel.Modules.LogCollector.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdministratorPanel.Infrastructure.LogCollector.Repositories
{
    public sealed class DefaultServerGroupRepository : IServerGroupRepository
    {
        private readonly IReadOnlyCollection<ServerGroup> _groups;

        public DefaultServerGroupRepository()
        {
            _groups = DefaultServerGroups.Create();
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
            {
                return null;
            }

            return _groups.FirstOrDefault(x =>
                string.Equals(x.Name, groupName, StringComparison.OrdinalIgnoreCase));
        }
    }
}
