using CommunityToolkit.Mvvm.ComponentModel;

namespace AdministratorPanel.UI.ViewModels.ServerMonitoring
{
    public partial class ServerMonitoringItemViewModel
        : ObservableObject
    {
        [ObservableProperty]
        private string _serverName = string.Empty;

        [ObservableProperty]
        private string _ipAddress = string.Empty;

        [ObservableProperty]
        private string _status = string.Empty;

        [ObservableProperty]
        private string _uptime = string.Empty;

        [ObservableProperty]
        private string _cpuUsage = string.Empty;

        [ObservableProperty]
        private string _memoryUsage = string.Empty;

        [ObservableProperty]
        private string _diskUsage = string.Empty;

        [ObservableProperty]
        private string _loadAverage = string.Empty;

        [ObservableProperty]
        private int _runningContainers;

        [ObservableProperty]
        private bool _dockerAvailable;

        [ObservableProperty]
        private string _error = string.Empty;

        [ObservableProperty]
        private bool _isCritical;

        public bool HasError =>
            !string.IsNullOrWhiteSpace(Error);

        public string StatusColor
        {
            get
            {
                if (Status == "Offline")
                    return "#DC2626";

                if (IsCritical)
                    return "#EA580C";

                return "#16A34A";
            }
        }
    }
}