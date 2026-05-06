using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdministratorPanel.Modules.ServerManagement.Models.ShutdownPlans
{
    public sealed class ShutdownPlanGroupDto
    {
        public int Order { get; init; }

        public string Name { get; init; } = string.Empty;

        public IReadOnlyCollection<ShutdownPlanServerDto> Servers { get; init; }
            = Array.Empty<ShutdownPlanServerDto>();

        public string DisplayName => $"{Order}. {Name} — серверов: {Servers.Count}";
    }
}
