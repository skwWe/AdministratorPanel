using AdministratorPanel.Modules.LogCollector.Abstractions;
using AdministratorPanel.Modules.LogCollector.Enums;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace AdministratorPanel.UI.ViewModels.LogCollector;

public partial class LogCollectorViewModel : ViewModelBase
{
    private readonly ILogCollectorWorkspaceService _workspaceService;
    private readonly ILogScriptRunner _logScriptRunner;

    [ObservableProperty]
    private ServerGroupItemViewModel? _selectedGroup;

    [ObservableProperty]
    private RemoteServerItemViewModel? _selectedServer;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private string _executionOutput = string.Empty;

    [ObservableProperty]
    private string _executionError = string.Empty;

    [ObservableProperty]
    private string _statusMessage = "Готово к запуску.";

    [ObservableProperty]
    private bool _isRunning;

    [ObservableProperty]
    private bool _isWslAvailable;

    [ObservableProperty]
    private string _wslStatusMessage = string.Empty;

    [ObservableProperty]
    private string _sshUserName = "Kuznetsov.SS";

    private bool _isWslMissing;

    public bool IsWslMissing
    {
        get => _isWslMissing;
        private set => SetProperty(ref _isWslMissing, value);
    }

    public LogCollectorViewModel(
        ILogCollectorWorkspaceService workspaceService,
        ILogScriptRunner logScriptRunner)
    {
        _workspaceService = workspaceService;
        _logScriptRunner = logScriptRunner;

        Groups = new ObservableCollection<ServerGroupItemViewModel>();
        Servers = new ObservableCollection<RemoteServerItemViewModel>();

        CheckWslAvailability();
        Load();
    }

    public ObservableCollection<ServerGroupItemViewModel> Groups { get; }

    public ObservableCollection<RemoteServerItemViewModel> Servers { get; }

    public bool CanRun => SelectedGroup is not null && !IsRunning && IsWslAvailable;

    partial void OnSelectedGroupChanged(ServerGroupItemViewModel? value)
    {
        Servers.Clear();

        if (value is null)
        {
            SelectedServer = null;
            OnPropertyChanged(nameof(CanRun));
            return;
        }

        foreach (var server in value.Servers.OrderBy(x => x.DisplayName))
        {
            Servers.Add(server);
        }

        SelectedServer = Servers.FirstOrDefault();
        OnPropertyChanged(nameof(CanRun));
    }

    partial void OnIsRunningChanged(bool value)
    {
        OnPropertyChanged(nameof(CanRun));
        RunCollectionCommand.NotifyCanExecuteChanged();
    }

    partial void OnIsWslAvailableChanged(bool value)
    {
        IsWslMissing = !value;

        OnPropertyChanged(nameof(CanRun));
        RunCollectionCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand(CanExecute = nameof(GetCanRunCollection))]
    private async Task RunCollectionAsync()
    {
        if (!IsWslAvailable)
        {
            StatusMessage = "WSL не установлен или не настроен.";
            return;
        }

        if (SelectedGroup is null)
        {
            StatusMessage = "Не выбрана группа серверов.";
            return;
        }

        IsRunning = true;
        ExecutionOutput = string.Empty;
        ExecutionError = string.Empty;
        StatusMessage = $"Запуск сбора логов для группы {SelectedGroup.Name}...";

        try
        {
            var groupType = MapGroupNameToEnum(SelectedGroup.Name);
            if (string.IsNullOrWhiteSpace(SshUserName))
            {
                StatusMessage = "Укажи SSH-пользователя.";
                return;
            }

            if (string.IsNullOrWhiteSpace(Password))
            {
                StatusMessage = "Укажи пароль.";
                return;
            }
            var progress = new Progress<AdministratorPanel.Modules.LogCollector.Models.LogScriptProgressMessage>(message =>
            {
                if (message.IsError)
                {
                    ExecutionError += message.Text + Environment.NewLine;
                }
                else
                {
                    ExecutionOutput += message.Text + Environment.NewLine;
                }
            });

            StatusMessage = $"Логин: {SshUserName}, пароль пустой: {string.IsNullOrWhiteSpace(Password)}";

            var result = await _logScriptRunner.RunAsync(
                groupType,
                SshUserName,
                Password,
                progress);

            ExecutionOutput = string.Empty;
            ExecutionError = string.Empty;
            StatusMessage = $"Запуск сбора логов для группы {SelectedGroup.Name}...";

            StatusMessage = result.IsSuccess
                ? $"Сбор логов завершён успешно. Код выхода: {result.ExitCode}"
                : $"Сбор логов завершён с ошибкой. Код выхода: {result.ExitCode}";
        }
        catch (Exception ex)
        {
            ExecutionError = ex.ToString();
            StatusMessage = "Во время запуска произошла ошибка.";
        }
        finally
        {
            IsRunning = false;
        }
    }

    [RelayCommand]
    private void CheckWsl()
    {
        CheckWslAvailability();
    }

    [RelayCommand]
    private void InstallWsl()
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "powershell",
                Arguments = "Start-Process powershell -Verb RunAs -ArgumentList 'wsl --install'",
                UseShellExecute = true
            };

            Process.Start(psi);

            WslStatusMessage = "Запущена установка WSL. После установки может потребоваться перезагрузка и настройка Linux-дистрибутива.";
        }
        catch (Exception ex)
        {
            WslStatusMessage = $"Не удалось запустить установку WSL: {ex.Message}";
        }
    }

    [RelayCommand]
    private void OpenWslHelp()
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://learn.microsoft.com/windows/wsl/install",
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            WslStatusMessage = $"Не удалось открыть инструкцию: {ex.Message}";
        }
    }

    private bool GetCanRunCollection()
    {
        return SelectedGroup is not null && !IsRunning && IsWslAvailable;
    }

    private void Load()
    {
        Groups.Clear();

        var groups = _workspaceService.GetServerGroups();

        foreach (var group in groups)
        {
            Groups.Add(new ServerGroupItemViewModel
            {
                Id = group.Id,
                Name = group.Name,
                Description = group.Description,
                Servers = new ObservableCollection<RemoteServerItemViewModel>(
                    group.Servers.Select(server => new RemoteServerItemViewModel
                    {
                        Id = server.Id,
                        DisplayName = server.DisplayName,
                        IpAddress = server.IpAddress,
                        IsEnabled = server.IsEnabled
                    }))
            });
        }

        SelectedGroup = Groups.FirstOrDefault();
    }

    private void CheckWslAvailability()
    {
        try
        {
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "wsl.exe",
                    Arguments = "--list --quiet",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();

            var output = process.StandardOutput.ReadToEnd();
            var error = process.StandardError.ReadToEnd();

            process.WaitForExit();

            var installedDistros = output
                .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToArray();

            IsWslAvailable = process.ExitCode == 0 && installedDistros.Length > 0;

            if (IsWslAvailable)
            {
                WslStatusMessage = $"WSL обнаружен. Установленные дистрибутивы: {string.Join(", ", installedDistros)}";
            }
            else
            {
                WslStatusMessage = string.IsNullOrWhiteSpace(error)
                    ? "WSL найден, но Linux-дистрибутив не установлен. Выполни установку WSL и настрой дистрибутив."
                    : $"WSL недоступен или не настроен: {error}";
            }

            IsWslMissing = !IsWslAvailable;
        }
        catch (Exception ex)
        {
            IsWslAvailable = false;
            IsWslMissing = true;
            WslStatusMessage = $"WSL не найден в системе: {ex.Message}";
        }
    }

    private static LogCollectionGroupType MapGroupNameToEnum(string groupName)
    {
        return groupName.Trim().ToLowerInvariant() switch
        {
            "app" => LogCollectionGroupType.App,
            "convert" => LogCollectionGroupType.Convert,
            "sync" => LogCollectionGroupType.Sync,
            "web" => LogCollectionGroupType.Web,
            _ => throw new InvalidOperationException($"Неизвестная группа серверов: {groupName}")
        };
    }
}