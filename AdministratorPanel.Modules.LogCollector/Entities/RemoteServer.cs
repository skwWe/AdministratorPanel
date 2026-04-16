using AdministratorPanel.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdministratorPanel.Modules.LogCollector.Entities
{
    public sealed class RemoteServer : BaseEntity
    {
        public string IpAddress { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public bool IsEnabled { get; set; } = true;

        public int SortOrder { get; set; }

        public Guid ServerGroupId { get; set; }
    }
}
