using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdministratorPanel.Modules.LogCollector.Models
{
    public sealed class LogScriptProgressMessage
    {
        public bool IsError { get; init; }

        public string Text { get; init; } = string.Empty;
    }
}
