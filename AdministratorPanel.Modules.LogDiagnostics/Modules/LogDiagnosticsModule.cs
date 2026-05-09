using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdministratorPanel.Core.Abstractions;
using AdministratorPanel.Core.Entities;
using AdministratorPanel.Core.Enums;
using System.Threading.Tasks;

namespace AdministratorPanel.Modules.LogDiagnostics.Modules
{
    public sealed class LogDiagnosticsModule : IToolModule
    {
        public string ModuleKey => "log-diagnostics";

        public AdminTool GetToolInfo()
        {
            return new AdminTool
            {
                Name = "Диагностика логов",
                Description = "Анализ собранных Docker‑логов: поиск ошибок, отчёты, статистика.",
                IconKey = "Chart",
                Type = ToolType.LogCollector,
                AvailabilityStatus = ToolAvailabilityStatus.Available,
                IsEnabled = true,
                SortOrder = 3
            };
        }
    }
}
