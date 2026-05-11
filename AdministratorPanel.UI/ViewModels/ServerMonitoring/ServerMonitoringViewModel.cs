using AdministratorPanel.Infrastructure.Security;
using AdministratorPanel.Modules.LogCollector.Abstractions;
using AdministratorPanel.Modules.ServerMonitoring.Abstractions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AdministratorPanel.UI.ViewModels.ServerMonitoring
{
    public partial class ServerMonitoringViewModel : ViewModelBase
    {
        private readonly ILogCollectorWorkspaceService _workspaceService;
        private readonly IServerMonitoringService _monitoringService;
        private readonly ISshSessionService _session;
        private PeriodicTimer? _timer;
        private bool _initialized;
        private readonly SemaphoreSlim _refreshLock = new(1, 1);

        [ObservableProperty]
        private string _statusMessage = "Система мониторинга готова.";

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private bool _isRealtimeEnabled = true;

        [ObservableProperty]
        private DateTime _lastUpdateTime;

        [ObservableProperty]
        private int _onlineServers;

        [ObservableProperty]
        private int _offlineServers;

        [ObservableProperty]
        private int _criticalServers;

        [ObservableProperty]
        private ServerMonitoringGroupViewModel? _selectedGroup;

        public ObservableCollection<ServerMonitoringGroupViewModel> Groups { get; } = new();
        public ObservableCollection<ServerMonitoringItemViewModel> VisibleServers { get; } = new();

        // Вычисляемые свойства для привязки (без сложных выражений в XAML)
        public string LastRefreshInfo =>
            $"Обновление каждые 10 сек | Последнее: {LastUpdateTime:HH:mm:ss}";

        public int TotalServers => OnlineServers + OfflineServers;

        public ServerMonitoringViewModel(
            ILogCollectorWorkspaceService workspaceService,
            IServerMonitoringService monitoringService,
            ISshSessionService session)
        {
            _workspaceService = workspaceService;
            _monitoringService = monitoringService;
            _session = session;
        }

        public async Task InitializeAsync()
        {
            if (_initialized) return;
            _initialized = true;
            await RefreshAsync();
            StartRealtimeMonitoring();
        }

        partial void OnSelectedGroupChanged(ServerMonitoringGroupViewModel? value)
        {
            VisibleServers.Clear();
            if (value is null) return;
            foreach (var server in value.Servers)
                VisibleServers.Add(server);
        }

        [RelayCommand]
        private void SelectGroup(ServerMonitoringGroupViewModel group)
        {
            SelectedGroup = group;
        }

        [RelayCommand]
        public async Task RefreshAsync()
        {
            if (IsLoading) return;
            if (string.IsNullOrWhiteSpace(_session.UserName))
            {
                StatusMessage = "SSH пользователь не авторизован.";
                return;
            }
            if (string.IsNullOrWhiteSpace(_session.Password))
            {
                StatusMessage = "SSH пароль не авторизован.";
                return;
            }

            await _refreshLock.WaitAsync();
            try
            {
                IsLoading = true;
                StatusMessage = "Обновление мониторинга...";

                var workspaceGroups = _workspaceService.GetServerGroups();
                var updatedGroups = new List<ServerMonitoringGroupViewModel>();
                int online = 0, offline = 0, critical = 0;

                foreach (var group in workspaceGroups)
                {
                    var groupVm = new ServerMonitoringGroupViewModel { GroupName = group.Name };
                    var semaphore = new SemaphoreSlim(10);
                    var tasks = group.Servers
                        .Where(x => x.IsEnabled)
                        .Select(async server =>
                        {
                            await semaphore.WaitAsync();
                            try
                            {
                                var result = await _monitoringService.GetMonitoringInfoAsync(
                                    server.DisplayName,
                                    server.IpAddress,
                                    _session.UserName,
                                    _session.Password);

                                bool isCritical =
                                    ParsePercent(result.CpuUsage) >= 80 ||
                                    ParsePercent(result.MemoryUsage) >= 90 ||
                                    ParsePercent(result.DiskUsage) >= 90;

                                if (result.Status.ToString() == "Online")
                                    Interlocked.Increment(ref online);
                                else
                                    Interlocked.Increment(ref offline);

                                if (isCritical)
                                    Interlocked.Increment(ref critical);

                                return new ServerMonitoringItemViewModel
                                {
                                    ServerName = result.ServerName,
                                    IpAddress = result.IpAddress,
                                    Status = result.Status.ToString(),
                                    Uptime = result.Uptime,
                                    CpuUsage = result.CpuUsage,
                                    MemoryUsage = result.MemoryUsage,
                                    DiskUsage = result.DiskUsage,
                                    LoadAverage = result.LoadAverage,
                                    RunningContainers = result.RunningContainers,
                                    DockerAvailable = result.DockerAvailable,
                                    Error = result.Error,
                                    IsCritical = isCritical
                                };
                            }
                            finally
                            {
                                semaphore.Release();
                            }
                        });

                    var servers = await Task.WhenAll(tasks);
                    foreach (var srv in servers)
                        groupVm.Servers.Add(srv);

                    updatedGroups.Add(groupVm);
                }

                Groups.Clear();
                foreach (var g in updatedGroups) Groups.Add(g);

                SelectedGroup = Groups.FirstOrDefault(x => x.GroupName == SelectedGroup?.GroupName)
                                ?? Groups.FirstOrDefault();

                OnlineServers = online;
                OfflineServers = offline;
                CriticalServers = critical;
                LastUpdateTime = DateTime.Now;
                OnPropertyChanged(nameof(LastRefreshInfo));
                OnPropertyChanged(nameof(TotalServers));

                StatusMessage = $"Мониторинг обновлён. Серверов: {TotalServers}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка мониторинга: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
                _refreshLock.Release();
            }
        }

        private void StartRealtimeMonitoring()
        {
            if (_timer is not null) return;
            _timer = new PeriodicTimer(TimeSpan.FromSeconds(10));
            _ = Task.Run(async () =>
            {
                while (await _timer.WaitForNextTickAsync())
                {
                    try
                    {
                        if (IsRealtimeEnabled)
                            await RefreshAsync();
                    }
                    catch { }
                }
            });
        }

        private static int ParsePercent(string value)
        {
            try
            {
                string digits = new string(value.Where(char.IsDigit).ToArray());
                return int.TryParse(digits, out int result) ? result : 0;
            }
            catch { return 0; }
        }
    }
}