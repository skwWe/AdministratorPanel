using AdministratorPanel.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdministratorPanel.Application.Models
{
    public sealed class ToolRegistrationResult
    {
        public bool IsSuccess { get; init; }

        public string Message { get; init; } = string.Empty;

        public string ModuleKey { get; init; } = string.Empty;

        public AdminTool? Tool { get; init; }

        public static ToolRegistrationResult Success(string moduleKey, AdminTool tool, string message = "Модуль зарегистрирован")
        {
            return new ToolRegistrationResult
            {
                IsSuccess = true,
                ModuleKey = moduleKey,
                Tool = tool,
                Message = message
            };
        }

        public static ToolRegistrationResult Failure(string moduleKey, string message)
        {
            return new ToolRegistrationResult
            {
                IsSuccess = false,
                ModuleKey = moduleKey,
                Message = message
            };
        }
    }
}
