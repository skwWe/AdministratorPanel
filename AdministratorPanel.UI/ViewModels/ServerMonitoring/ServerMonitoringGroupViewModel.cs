using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace AdministratorPanel.UI.ViewModels.ServerMonitoring
{
    public partial class ServerMonitoringGroupViewModel
        : ObservableObject
    {
        [ObservableProperty]
        private string _groupName = string.Empty;

        public ObservableCollection<
            ServerMonitoringItemViewModel>
            Servers
        { get; } = new();
    }
}