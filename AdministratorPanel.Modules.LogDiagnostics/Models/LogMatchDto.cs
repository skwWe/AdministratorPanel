using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdministratorPanel.Modules.LogDiagnostics.Models
{
    public sealed class LogMatchDto
    {
        public string ServerIp { get; init; } = string.Empty;
        public string LogFile { get; init; } = string.Empty;
        public int LineNumber { get; init; }
        public string LineText { get; init; } = string.Empty;
    }
}
