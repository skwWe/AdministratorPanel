using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdministratorPanel.Modules.LogDiagnostics.Models
{
    public sealed class SessionInfoDto
    {
        public string GroupName { get; init; } = string.Empty;
        public string Timestamp { get; init; } = string.Empty;
        public string FullPath { get; init; } = string.Empty;
        public int ServerCount { get; init; }
        public string DisplayName => $"{GroupName} / {Timestamp}";
    }
}
