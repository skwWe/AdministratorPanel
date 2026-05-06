using AdministratorPanel.Modules.ServerManagement.Abstractions;
using AdministratorPanel.Modules.ServerManagement.Models.ShutdownPlans;

namespace AdministratorPanel.Modules.ServerManagement.Services
{
    public sealed class DefaultShutdownRuleProvider : IShutdownRuleProvider
    {
        public ShutdownGroupRuleDto GetRule(string groupName)
        {
            var normalizedGroupName = groupName.Trim().ToLowerInvariant();

            return normalizedGroupName switch
            {
                "dev" => NoCompose("dev"),
                "intgr" => NoCompose("intgr"),

                _ => new ShutdownGroupRuleDto
                {
                    GroupName = normalizedGroupName,
                    HasDockerCompose = true,
                    ComposeDirectory = "/digdes/docker_data"
                }
            };
        }

        private static ShutdownGroupRuleDto NoCompose(string groupName)
        {
            return new ShutdownGroupRuleDto
            {
                GroupName = groupName,
                HasDockerCompose = false,
                ComposeDirectory = "/digdes/docker_data"
            };
        }
    }
}