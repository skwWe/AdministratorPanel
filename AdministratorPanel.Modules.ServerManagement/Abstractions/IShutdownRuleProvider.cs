using AdministratorPanel.Modules.ServerManagement.Models.ShutdownPlans;

namespace AdministratorPanel.Modules.ServerManagement.Abstractions
{
    public interface IShutdownRuleProvider
    {
        ShutdownGroupRuleDto GetRule(string groupName);
    }
}