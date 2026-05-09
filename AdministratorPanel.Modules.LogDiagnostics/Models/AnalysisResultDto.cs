using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdministratorPanel.Modules.LogDiagnostics.Models
{
    public sealed class AnalysisResultDto
    {
        public int TotalMatches { get; set; }
        public List<LogMatchDto> Matches { get; set; } = new();
    }
}
