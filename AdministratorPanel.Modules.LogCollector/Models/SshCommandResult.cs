using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdministratorPanel.Modules.LogCollector.Models
{
    public sealed class SshCommandResult
    {
        public bool IsSuccess => ExitCode == 0;

        public int ExitCode { get; init; }

        public string Output { get; init; } = string.Empty;

        public string Error { get; init; } = string.Empty;
    }
}
