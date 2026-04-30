using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdministratorPanel.Modules.LogCollector.Models
{
    public sealed class DiscoveredServerDto
    {
        public string DisplayName { get; init; } = string.Empty;

        public string IpAdress {  get; init; } = string.Empty;
    }
}
