using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdministratorPanel.Modules.ServerManagement.Models.ShutdownPlans
{
    public sealed class ShutdownPlanDto
    {
        public string Name { get; init; } = string.Empty;

        public string SourceFilePath { get; init; } = string.Empty;

        public DateTime LoadedAt { get; init; } = DateTime.Now;

        public IReadOnlyCollection<ShutdownPlanGroupDto> Groups { get; init; }
            = Array.Empty<ShutdownPlanGroupDto>();

        public int TotalServers => Groups.Sum(x => x.Servers.Count);
    }
}
