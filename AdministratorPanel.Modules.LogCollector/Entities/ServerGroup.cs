using AdministratorPanel.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdministratorPanel.Modules.LogCollector.Entities
{
    public sealed class ServerGroup : BaseEntity
    {
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public int SortOrder { get; set; }

        public List<RemoteServer> Servers { get; set; } = new();
    }
}
