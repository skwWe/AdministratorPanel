using AdministratorPanel.Modules.LogDiagnostics.Abstractions;
using AdministratorPanel.Modules.LogDiagnostics.Models;
using System.Text.RegularExpressions;

namespace AdministratorPanel.Modules.LogDiagnostics.Services;

public sealed class LogAnalysisService : ILogAnalysisService
{
    public Task<AnalysisResultDto> AnalyzeSessionAsync(string sessionPath, string searchPattern)
    {
        return Task.Run(() =>
        {
            var result = new AnalysisResultDto();
            if (!Directory.Exists(sessionPath)) return result;

            var regex = new Regex(searchPattern, RegexOptions.IgnoreCase);
            var matches = new List<LogMatchDto>();

            foreach (var serverDir in Directory.GetDirectories(sessionPath))
            {
                var ip = Path.GetFileName(serverDir);
                var logFile = Path.Combine(serverDir, "all.log");
                if (!File.Exists(logFile)) continue;

                var lines = File.ReadAllLines(logFile);
                for (int i = 0; i < lines.Length; i++)
                {
                    if (regex.IsMatch(lines[i]))
                    {
                        matches.Add(new LogMatchDto
                        {
                            ServerIp = ip,
                            LogFile = "all.log",
                            LineNumber = i + 1,
                            LineText = lines[i]
                        });
                    }
                }
            }

            result.TotalMatches = matches.Count;
            result.Matches = matches;
            return result;
        });
    }
}