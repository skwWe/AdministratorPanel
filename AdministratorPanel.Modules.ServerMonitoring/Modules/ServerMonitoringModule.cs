using AdministratorPanel.Core.Abstractions;
using AdministratorPanel.Core.Entities;
using AdministratorPanel.Core.Enums;

namespace AdministratorPanel.Modules.ServerMonitoring.Modules
{
    public sealed class ServerMonitoringModule : IToolModule
    {
        public string ModuleKey => "server-monitoring";

        public AdminTool GetToolInfo()
        {
            return new AdminTool
            {
                Name = "Мониторинг серверов",
                Description =
                    "Мониторинг Linux-серверов через удалённые Bash-скрипты.",
                IconKey = "Activity",
                Type = ToolType.ServerMonitoring,
                AvailabilityStatus =
                    ToolAvailabilityStatus.Available,
                IsEnabled = true,
                SortOrder = 4
            };
        }
    }
}