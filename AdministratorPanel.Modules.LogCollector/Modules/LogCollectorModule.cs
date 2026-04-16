using AdministratorPanel.Core.Abstractions;
using AdministratorPanel.Core.Entities;
using AdministratorPanel.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdministratorPanel.Modules.LogCollector.Modules
{
    public sealed class LogCollectorModule : IToolModule
    {
        public string ModuleKey => "log-collector";

        public AdminTool GetToolInfo()
        {
            return new AdminTool
            {
                Name = "Сбор логов Docker",
                Description = "Инструмент централизованного сбора, просмотра и диагностики Docker-логов удалённых серверов.",
                IconKey = "FileText",
                Type = ToolType.LogCollector,
                AvailabilityStatus = ToolAvailabilityStatus.Available,
                IsEnabled = true,
                SortOrder = 1
            };
        }
    }
}
