using AdministratorPanel.Core.Abstractions;
using AdministratorPanel.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdministratorPanel.Core.Enums;


namespace AdministratorPanel.Modules.ServerManagement.Modules
{
    public sealed class ServerManagementModule : IToolModule
    {
        public string ModuleKey => "server-management";

        public AdminTool GetToolInfo()
        {
            return new AdminTool
            {
                Name = "Управление серверами",
                Description = "Диагностика серверов, анализ Docker-сервисов, управление контейнерами и безопасное отключение.",
                IconKey = "Server",
                Type = ToolType.ServerManagement,
                AvailabilityStatus = ToolAvailabilityStatus.Available,
                IsEnabled = true,
                SortOrder = 2
            };
        }
    }
}
