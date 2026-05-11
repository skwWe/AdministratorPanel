using AdministratorPanel.UI.ViewModels;
using AdministratorPanel.UI.ViewModels.LogCollector;
using AdministratorPanel.UI.ViewModels.ServerManagement;
using AdministratorPanel.UI.ViewModels.ServerMonitoring;
using AdministratorPanel.UI.Views.LogCollector;
using AdministratorPanel.UI.Views.ServerManagement;
using AdministratorPanel.UI.Views.ServerMonitoring;
using Avalonia.Controls;

namespace AdministratorPanel.UI.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

    }


    public MainWindow(
        MainWindowViewModel viewModel,
        LogCollectorViewModel logCollectorViewModel,
        ServerManagementViewModel serverManagementViewModel,
        ServerMonitoringViewModel monitoringViewModel) : this()
    {
        DataContext = viewModel;

        var logCollectorView = this.FindControl<LogCollectorView>("LogCollectorHost");
        if (logCollectorView is not null)
            logCollectorView.DataContext = logCollectorViewModel;

        var serverManagementView = this.FindControl<ServerManagementView>("ServerManagementHost");
        if (serverManagementView is not null)
            serverManagementView.DataContext = serverManagementViewModel;

        var monitoringView = this.FindControl<ServerMonitoringView>("ServerMonitoringHost");
        if (monitoringView is not null)
            monitoringView.DataContext = monitoringViewModel;
    }
}