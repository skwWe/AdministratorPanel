using AdministratorPanel.Core.Abstractions;
using AdministratorPanel.Application.Models;
using AdministratorPanel.Core.Entities;
using AdministratorPanel.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdministratorPanel.Application.Abstractions
{
    public interface IToolRegistry
    {
        ToolRegistrationResult RegisterModule(IToolModule module);

        IReadOnlyCollection<AdminTool> GetAllTools();

        IReadOnlyCollection<AdminTool> GetEnabledTools();

        AdminTool? GetByType(ToolType toolType);

        AdminTool? GetByName(string name);

        bool ContainsModule(string moduleKey);
    }
}
