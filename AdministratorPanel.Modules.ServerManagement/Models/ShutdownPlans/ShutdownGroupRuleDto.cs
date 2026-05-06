using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdministratorPanel.Modules.ServerManagement.Models.ShutdownPlans
{
    public sealed class ShutdownGroupRuleDto
    {
        public string GroupName { get; init; } = string.Empty;

        public bool HasDockerCompose { get; init; } = true;

        public string ComposeDirectory { get; init; } = "/digdes/docker_data";
    }
}
