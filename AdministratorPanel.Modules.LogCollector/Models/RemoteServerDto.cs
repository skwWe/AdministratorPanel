using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdministratorPanel.Modules.LogCollector.Models
{
    public sealed class RemoteServerDto
    {
        public Guid Id { get; init; }

        public string IpAddress { get; init; } = string.Empty;

        public string DisplayName { get; init; } = string.Empty;

        public bool IsEnabled { get; init; }

        public int SortOrder { get; init; }
    }
}
