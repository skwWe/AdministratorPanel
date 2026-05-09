using AdministratorPanel.Modules.LogDiagnostics.Abstractions;
using AdministratorPanel.Modules.LogDiagnostics.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Renci.SshNet;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AdministratorPanel.UI.ViewModels.LogDiagnostics;

public partial class LogDiagnosticsViewModel : ViewModelBase
{
    private readonly ILogAnalysisService _analysisService;

    [ObservableProperty]
    private string _statusMessage = "Выберите сессию";

    [ObservableProperty]
    private ObservableCollection<SessionInfoDto> _sessions = new();

    [ObservableProperty]
    private SessionInfoDto? _selectedSession;

    [ObservableProperty]
    private string _searchPattern = "ERROR|FATAL|Exception";

    [ObservableProperty]
    private string _analysisResult = string.Empty;

    [ObservableProperty]
    private bool _isAnalyzing;

    public LogDiagnosticsViewModel(ILogAnalysisService analysisService)
    {
        _analysisService = analysisService;
        LoadSessions();
    }

    [RelayCommand]
    private void LoadSessions()
    {
        Sessions.Clear();
        var logsRoot = Path.Combine(AppContext.BaseDirectory, "logs");
        if (!Directory.Exists(logsRoot)) return;

        foreach (var groupDir in Directory.GetDirectories(logsRoot))
        {
            var groupName = Path.GetFileName(groupDir);
            foreach (var sessionDir in Directory.GetDirectories(groupDir))
            {
                Sessions.Add(new SessionInfoDto
                {
                    GroupName = groupName,
                    Timestamp = Path.GetFileName(sessionDir),
                    FullPath = sessionDir,
                    ServerCount = Directory.GetDirectories(sessionDir).Length
                });
            }
        }

        StatusMessage = $"Загружено {Sessions.Count} сессий.";
    }

    [RelayCommand]
    private async Task AnalyzeAsync()
    {
        if (SelectedSession == null)
        {
            StatusMessage = "Выберите сессию.";
            return;
        }

        IsAnalyzing = true;
        AnalysisResult = "Анализ...";
        try
        {
            var result = await _analysisService.AnalyzeSessionAsync(SelectedSession.FullPath, SearchPattern);
            if (result.TotalMatches == 0)
                AnalysisResult = "Совпадений не найдено.";
            else
                AnalysisResult = $"Найдено {result.TotalMatches} совпадений.\n\n" +
                                 string.Join("\n", result.Matches.Take(50).Select(m => $"[{m.ServerIp}:{m.LineNumber}] {m.LineText}"));
        }
        catch (Exception ex)
        {
            AnalysisResult = $"Ошибка: {ex.Message}";
        }
        finally
        {
            IsAnalyzing = false;
            StatusMessage = "Анализ завершён.";
        }
    }
}