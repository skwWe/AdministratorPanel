using AdministratorPanel.Infrastructure.Security;
using AdministratorPanel.Modules.ServerManagement.Abstractions;
using AdministratorPanel.Modules.ServerManagement.Models.Operations;
using AdministratorPanel.UI.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace AdministratorPanel.UI.ViewModels.ServerManagement
{
    public partial class ServerManagementViewModel : ViewModelBase
    {
        private readonly IShutdownPlanImportService _shutdownPlanImportService;
        private readonly IFileDialogService _fileDialogService;
        private readonly IServerComposeService _serverComposeService;
        private readonly IServerShutdownService _serverShutdownService;
        private readonly ISshSessionService _session;
        private readonly IShutdownRuleProvider _shutdownRuleProvider;

        [ObservableProperty]
        private string _statusMessage = "Модуль управления серверами готов.";

        [ObservableProperty]
        private string _sshUserName = "Kuznetsov.SS";

        [ObservableProperty]
        private string _password = string.Empty;

        [ObservableProperty]
        private string _serverName = string.Empty;

        [ObservableProperty]
        private string _ipAddress = string.Empty;

        [ObservableProperty]
        private string _operationOutput = string.Empty;

        [ObservableProperty]
        private string _operationError = string.Empty;

        [ObservableProperty]
        private string _shutdownPlanFilePath = string.Empty;

        [ObservableProperty]
        private string _loadedPlanName = "План отключения не загружен.";

        [ObservableProperty]
        private ShutdownPlanGroupItemViewModel? _selectedShutdownGroup;

        [ObservableProperty]
        private ShutdownPlanServerItemViewModel? _selectedShutdownServer;

        public ServerManagementViewModel(
            IShutdownPlanImportService shutdownPlanImportService,
            IFileDialogService fileDialogService,
            IServerComposeService serverComposeService,
            IServerShutdownService serverShutdownService,
            IShutdownRuleProvider shutdownRuleProvider,
            ISshSessionService session)
        {
            _shutdownPlanImportService = shutdownPlanImportService;
            _fileDialogService = fileDialogService;
            _serverComposeService = serverComposeService;
            _serverShutdownService = serverShutdownService;
            _shutdownRuleProvider = shutdownRuleProvider;
            _session = session;

            ShutdownGroups = new ObservableCollection<ShutdownPlanGroupItemViewModel>();
            ShutdownServers = new ObservableCollection<ShutdownPlanServerItemViewModel>();
        }

        public ObservableCollection<ShutdownPlanGroupItemViewModel> ShutdownGroups { get; }

        public ObservableCollection<ShutdownPlanServerItemViewModel> ShutdownServers { get; }

        partial void OnSelectedShutdownGroupChanged(ShutdownPlanGroupItemViewModel? value)
        {
            ShutdownServers.Clear();

            if (value is null)
            {
                SelectedShutdownServer = null;
                return;
            }

            foreach (var server in value.Servers.OrderBy(x => x.Order))
            {
                ShutdownServers.Add(server);
            }

            SelectedShutdownServer = ShutdownServers.FirstOrDefault();

            StatusMessage = $"Выбрана группа {value.Name}. Серверов: {value.Servers.Count}.";
        }

        partial void OnSelectedShutdownServerChanged(ShutdownPlanServerItemViewModel? value)
        {
            if (value is null)
                return;

            ServerName = string.IsNullOrWhiteSpace(value.HostName)
                ? value.DomainName
                : value.HostName;

            IpAddress = value.IpAddress;

            StatusMessage = $"Выбран сервер: {value.DisplayName}";
        }

        [RelayCommand]
        private async Task BrowseShutdownPlanFileAsync()
        {
            var filePath = await _fileDialogService.PickExcelFileAsync();

            if (string.IsNullOrWhiteSpace(filePath))
            {
                StatusMessage = "Файл плана отключения не выбран.";
                return;
            }

            ShutdownPlanFilePath = filePath;
            StatusMessage = $"Выбран файл плана: {filePath}";
        }

        [RelayCommand]
        private async Task LoadShutdownPlanAsync()
        {
            OperationError = string.Empty;
            OperationOutput = string.Empty;

            if (string.IsNullOrWhiteSpace(ShutdownPlanFilePath))
            {
                StatusMessage = "Укажи путь к Excel-файлу плана отключения.";
                return;
            }

            try
            {
                StatusMessage = "Загрузка плана отключения...";

                var plan = await _shutdownPlanImportService.ImportAsync(ShutdownPlanFilePath);

                ShutdownGroups.Clear();
                ShutdownServers.Clear();

                foreach (var group in plan.Groups.OrderBy(x => x.Order))
                {
                    ShutdownGroups.Add(ShutdownPlanGroupItemViewModel.FromDto(group));
                }

                SelectedShutdownGroup = ShutdownGroups.FirstOrDefault();

                LoadedPlanName = $"{plan.Name}. Групп: {plan.Groups.Count}. Серверов: {plan.TotalServers}.";

                OperationOutput =
                    $"План загружен: {plan.Name}{Environment.NewLine}" +
                    $"Файл: {plan.SourceFilePath}{Environment.NewLine}" +
                    $"Дата загрузки: {plan.LoadedAt:yyyy-MM-dd HH:mm:ss}{Environment.NewLine}" +
                    $"Групп: {plan.Groups.Count}{Environment.NewLine}" +
                    $"Серверов: {plan.TotalServers}{Environment.NewLine}";

                StatusMessage = "План отключения успешно загружен.";
            }
            catch (Exception ex)
            {
                OperationError = ex.ToString();
                StatusMessage = "Ошибка загрузки плана отключения.";
            }
        }

        [RelayCommand]
        private void UseSelectedPlanServer()
        {
            if (SelectedShutdownServer is null)
            {
                StatusMessage = "Выбери сервер из плана.";
                return;
            }

            ServerName = string.IsNullOrWhiteSpace(SelectedShutdownServer.HostName)
                ? SelectedShutdownServer.DomainName
                : SelectedShutdownServer.HostName;

            IpAddress = SelectedShutdownServer.IpAddress;

            StatusMessage = $"Сервер из плана выбран для операции: {ServerName} / {IpAddress}.";
        }

        [RelayCommand]
        private async Task RestartServicesAsync()
        {
            if (!CanExecuteServerOperation())
                return;

            OperationOutput = string.Empty;
            OperationError = string.Empty;
            StatusMessage = $"Перезапуск сервисов на сервере {ServerName}...";

            try
            {
                var result = await _serverComposeService.RestartServicesAsync(
                    ServerName,
                    IpAddress,
                    _session.UserName,
                    _session.Password);

                ApplyOperationResult(result);
            }
            catch (Exception ex)
            {
                OperationError = ex.ToString();
                StatusMessage = "Ошибка при перезапуске сервисов.";
            }
        }

        [RelayCommand]
        private async Task SafeShutdownSelectedServerAsync()
        {
            if (!CanExecuteServerOperation())
                return;

            if (SelectedShutdownServer is null)
            {
                StatusMessage = "Выбери сервер из плана отключения.";
                return;
            }

            OperationOutput = string.Empty;
            OperationError = string.Empty;
            StatusMessage = $"Безопасное отключение сервера {ServerName}...";

            try
            {
                var rule = _shutdownRuleProvider.GetRule(SelectedShutdownServer.GroupName);

                var result = await _serverShutdownService.SafeShutdownAsync(
                    SelectedShutdownServer.GroupName,
                    ServerName,
                    IpAddress,
                    _session.UserName,
                    _session.Password,
                    rule);

                ApplyOperationResult(result);
            }
            catch (Exception ex)
            {
                OperationError = ex.ToString();
                StatusMessage = "Ошибка при безопасном отключении сервера.";
            }
        }

        private bool CanExecuteServerOperation()
        {
            if (string.IsNullOrWhiteSpace(ServerName))
            {
                StatusMessage = "Выбери сервер из плана или укажи имя сервера.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(IpAddress))
            {
                StatusMessage = "Укажи IP-адрес сервера.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(_session.UserName))
            {
                StatusMessage = "Укажи SSH-пользователя.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(_session.Password))
            {
                StatusMessage = "Укажи пароль.";
                return false;
            }

            return true;
        }

        private void ApplyOperationResult(ServerOperationResultDto result)
        {
            OperationOutput = result.Output;
            OperationError = result.Error;

            StatusMessage = result.IsSuccess
                ? $"{result.OperationName} выполнена успешно."
                : $"{result.OperationName} завершилась с ошибкой.";
        }
    }
}