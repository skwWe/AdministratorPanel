using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdministratorPanel.Modules.LogCollector.Models
{
    public sealed class LogScriptExecutionResult
    {
        public bool IsSuccess { get; init; }

        public int ExitCode { get; init; }

        public string StandardOutput { get; init; } = string.Empty;

        public string StandardError { get; init; } = string.Empty;

        public string ExecutedScriptPath { get; init; } = string.Empty;
    }
}
