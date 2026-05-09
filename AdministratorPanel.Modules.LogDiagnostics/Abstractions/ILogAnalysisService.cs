using AdministratorPanel.Modules.LogDiagnostics.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdministratorPanel.Modules.LogDiagnostics.Abstractions
{
    public interface ILogAnalysisService
    {
        Task<AnalysisResultDto> AnalyzeSessionAsync(string sessionPath, string searchPattern);
    }
}
