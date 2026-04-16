using AdministratorPanel.Application.Abstractions;
using AdministratorPanel.Application.Models;
using AdministratorPanel.Core.Abstractions;
using AdministratorPanel.Core.Entities;
using AdministratorPanel.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdministratorPanel.Application.Services
{
    public sealed class ToolRegistry : IToolRegistry, IToolProvider
    {
        private readonly Dictionary<string, AdminTool> _toolsByModuleKey = new(StringComparer.OrdinalIgnoreCase);

        public ToolRegistrationResult RegisterModule(IToolModule module)
        {
            ArgumentNullException.ThrowIfNull(module);

            if (string.IsNullOrWhiteSpace(module.ModuleKey))
            {
                return ToolRegistrationResult.Failure(string.Empty, "Module key cannot be null or empty.");
            }

            if (_toolsByModuleKey.ContainsKey(module.ModuleKey))
            {
                return ToolRegistrationResult.Failure(module.ModuleKey, $"Module '{module.ModuleKey}' is already registered.");
            }

            AdminTool toolInfo;

            try
            {
                toolInfo = module.GetToolInfo();
            }
            catch (Exception ex)
            {
                return ToolRegistrationResult.Failure(module.ModuleKey, $"Failed to get tool info: {ex.Message}");
            }

            if (toolInfo is null)
            {
                return ToolRegistrationResult.Failure(module.ModuleKey, "Tool info cannot be null.");
            }

            if (string.IsNullOrWhiteSpace(toolInfo.Name))
            {
                return ToolRegistrationResult.Failure(module.ModuleKey, "Tool name cannot be null or empty.");
            }

            _toolsByModuleKey[module.ModuleKey] = toolInfo;

            return ToolRegistrationResult.Success(module.ModuleKey, toolInfo);
        }

        public IReadOnlyCollection<AdminTool> GetAllTools()
        {
            return _toolsByModuleKey.Values
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.Name)
                .ToArray();
        }

        public IReadOnlyCollection<AdminTool> GetEnabledTools()
        {
            return _toolsByModuleKey.Values
                .Where(x => x.IsEnabled && x.AvailabilityStatus == ToolAvailabilityStatus.Available)
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.Name)
                .ToArray();
        }

        public AdminTool? GetByType(ToolType toolType)
        {
            return _toolsByModuleKey.Values.FirstOrDefault(x => x.Type == toolType);
        }

        public AdminTool? GetByName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return null;
            }

            return _toolsByModuleKey.Values.FirstOrDefault(x =>
                string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));
        }

        public bool ContainsModule(string moduleKey)
        {
            if (string.IsNullOrWhiteSpace(moduleKey))
            {
                return false;
            }

            return _toolsByModuleKey.ContainsKey(moduleKey);
        }

        public IReadOnlyCollection<AdminTool> GetTools()
        {
            return GetEnabledTools();
        }
    }
}
