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
    private readonly ISshCommandRunner _sshCommandRunner;
    private readonly IServerDiscoveryService _serverDiscoveryService;

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
    private string _sshUserName = "Kuznetsov.SS";

    [ObservableProperty]
    private string _serverDisplayName = string.Empty;

    [ObservableProperty]
    private string _serverIpAddress = string.Empty;

    [ObservableProperty]
    private bool _serverIsEnabled = true;

    public LogCollectorViewModel(
        ILogCollectorWorkspaceService workspaceService,
        ILogScriptRunner logScriptRunner,
        ISshCommandRunner sshCommandRunner,
        IServerDiscoveryService serverDiscoveryService)
    {
        _workspaceService = workspaceService;
        _logScriptRunner = logScriptRunner;
        _sshCommandRunner = sshCommandRunner;
        _serverDiscoveryService = serverDiscoveryService;

        Groups = new ObservableCollection<ServerGroupItemViewModel>();
        Servers = new ObservableCollection<RemoteServerItemViewModel>();

        Load();
    }

    public ObservableCollection<ServerGroupItemViewModel> Groups { get; }

    public ObservableCollection<RemoteServerItemViewModel> Servers { get; }

    public bool CanRun => SelectedGroup is not null && !IsRunning;

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
    }



    [RelayCommand(CanExecute = nameof(GetCanRunCollection))]
    private async Task RunCollectionAsync()
    {

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

    private bool GetCanRunCollection()
    {
        return SelectedGroup is not null && !IsRunning;
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

    partial void OnSelectedServerChanged(RemoteServerItemViewModel? value)
    {
        if (value is null)
        {
            ServerDisplayName = string.Empty;
            ServerIpAddress = string.Empty;
            ServerIsEnabled = true;
            return;
        }

        ServerDisplayName = value.DisplayName;
        ServerIpAddress = value.IpAddress;
        ServerIsEnabled = value.IsEnabled;
    }

    [RelayCommand]
    private void AddServer()
    {
        if (SelectedGroup is null)
        {
            StatusMessage = "Сначала выбери группу серверов.";
            return;
        }

        if (string.IsNullOrWhiteSpace(ServerDisplayName))
        {
            StatusMessage = "Укажи имя сервера.";
            return;
        }

        if (string.IsNullOrWhiteSpace(ServerIpAddress))
        {
            StatusMessage = "Укажи IP-адрес сервера.";
            return;
        }

        var server = _workspaceService.AddServer(
            SelectedGroup.Id,
            ServerDisplayName,
            ServerIpAddress,
            ServerIsEnabled);

        var item = new RemoteServerItemViewModel
        {
            Id = server.Id,
            DisplayName = server.DisplayName,
            IpAddress = server.IpAddress,
            IsEnabled = server.IsEnabled
        };

        SelectedGroup.Servers.Add(item);
        Servers.Add(item);
        SelectedServer = item;

        OnPropertyChanged(nameof(SelectedGroup.ServerCount));

        StatusMessage = $"Сервер {server.DisplayName} добавлен.";
    }

    [RelayCommand]
    private void UpdateServer()
    {
        if (SelectedServer is null)
        {
            StatusMessage = "Выбери сервер для редактирования.";
            return;
        }

        if (string.IsNullOrWhiteSpace(ServerDisplayName))
        {
            StatusMessage = "Укажи имя сервера.";
            return;
        }

        if (string.IsNullOrWhiteSpace(ServerIpAddress))
        {
            StatusMessage = "Укажи IP-адрес сервера.";
            return;
        }

        var updated = _workspaceService.UpdateServer(
            SelectedServer.Id,
            ServerDisplayName,
            ServerIpAddress,
            ServerIsEnabled);

        if (!updated)
        {
            StatusMessage = "Сервер не найден.";
            return;
        }

        SelectedServer.DisplayName = ServerDisplayName.Trim();
        SelectedServer.IpAddress = ServerIpAddress.Trim();
        SelectedServer.IsEnabled = ServerIsEnabled;

        StatusMessage = $"Сервер {SelectedServer.DisplayName} обновлён.";
    }

    [RelayCommand]
    private void DeleteServer()
    {
        if (SelectedServer is null)
        {
            StatusMessage = "Выбери сервер для удаления.";
            return;
        }

        var deletedServer = SelectedServer;

        var deleted = _workspaceService.DeleteServer(deletedServer.Id);

        if (!deleted)
        {
            StatusMessage = "Сервер не найден.";
            return;
        }

        Servers.Remove(deletedServer);
        SelectedGroup?.Servers.Remove(deletedServer);

        SelectedServer = Servers.FirstOrDefault();

        if (SelectedGroup is not null)
        {
            OnPropertyChanged(nameof(SelectedGroup.ServerCount));
        }

        StatusMessage = $"Сервер {deletedServer.DisplayName} удалён.";
    }

    [RelayCommand]
    private void ClearServerForm()
    {
        SelectedServer = null;
        ServerDisplayName = string.Empty;
        ServerIpAddress = string.Empty;
        ServerIsEnabled = true;
    }

    [RelayCommand]
    private async Task TestSshConnectionAsync()
    {
        if (SelectedServer is null)
        {
            StatusMessage = "Выбери сервер.";
            return;
        }

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

        IsRunning = true;
        ExecutionOutput = string.Empty;
        ExecutionError = string.Empty;
        StatusMessage = $"Проверка SSH-подключения к {SelectedServer.DisplayName}...";

        try
        {
            var result = await _sshCommandRunner.RunAsync(
                SelectedServer.IpAddress,
                SshUserName,
                Password,
                "hostname");

            ExecutionOutput = result.Output;
            ExecutionError = result.Error;

            StatusMessage = result.IsSuccess
                ? "SSH-подключение успешно."
                : $"SSH-команда завершилась с ошибкой. Код: {result.ExitCode}";
        }
        catch (Exception ex)
        {
            ExecutionError = ex.ToString();
            StatusMessage = "Не удалось выполнить SSH-подключение.";
        }
        finally
        {
            IsRunning = false;
        }
    }

    [RelayCommand]
    private async Task DiscoverServersAsync()
    {
        if (SelectedGroup is null)
        {
            StatusMessage = "Сначала выбери группу серверов.";
            return;
        }

        if (string.IsNullOrWhiteSpace(SshUserName))
        {
            StatusMessage = "Укажи SSH пользователя.";
            return;
        }

        if (string.IsNullOrWhiteSpace(Password))
        {
            StatusMessage = "Укажи пароль.";
            return;
        }

        var groupName = SelectedGroup.Name.Trim().ToLowerInvariant();

        IsRunning = true;
        StatusMessage = "Поиск серверов...";

        try
        {
            var discoveredServers = await _serverDiscoveryService.DiscoverAsync(
                "10.10.130",
                1,
                254,
                SshUserName,
                Password);

            var added = 0;
            var updated = 0;

            foreach (var server in discoveredServers)
            {
                var normalizedName = NormalizeHostName(server.DisplayName);

                // 🔥 ФИЛЬТР ПО ГРУППЕ
                if (!IsServerFromGroup(normalizedName, groupName))
                    continue;

                var existing = SelectedGroup.Servers.FirstOrDefault(x =>
                    string.Equals(x.DisplayName, normalizedName, StringComparison.OrdinalIgnoreCase));

                if (existing != null)
                {
                    if (existing.IpAddress != server.IpAddress)
                    {
                        existing.IpAddress = server.IpAddress;

                        _workspaceService.UpdateServer(
                            existing.Id,
                            existing.DisplayName,
                            existing.IpAddress,
                            existing.IsEnabled);

                        updated++;
                    }

                    continue;
                }

                var saved = _workspaceService.AddServer(
                    SelectedGroup.Id,
                    normalizedName,
                    server.IpAddress,
                    true);

                var vm = new RemoteServerItemViewModel
                {
                    Id = saved.Id,
                    DisplayName = saved.DisplayName,
                    IpAddress = saved.IpAddress,
                    IsEnabled = saved.IsEnabled
                };

                SelectedGroup.Servers.Add(vm);
                Servers.Add(vm);

                added++;
            }

            OnPropertyChanged(nameof(SelectedGroup.ServerCount));

            StatusMessage = $"Поиск завершён. Найдено: {discoveredServers.Count}. Добавлено: {added}. Обновлено: {updated}.";
        }
        catch (Exception ex)
        {
            ExecutionError = ex.ToString();
            StatusMessage = "Ошибка поиска серверов.";
        }
        finally
        {
            IsRunning = false;
        }
    }
    

    private static string NormalizeHostName(string hostName)
    {
        if (string.IsNullOrWhiteSpace(hostName))
            return hostName;

        // nn-lsed-web02.nnov.ru → nn-lsed-web02
        return hostName.Split('.')[0].Trim().ToLowerInvariant();
    }

    private static bool IsServerFromGroup(string hostName, string groupName)
    {
        var name = hostName.Trim().ToLowerInvariant();
        var group = groupName.Trim().ToLowerInvariant();

        // 🔥 строгая проверка
        return name.StartsWith($"nn-lsed-{group}");
    }
}