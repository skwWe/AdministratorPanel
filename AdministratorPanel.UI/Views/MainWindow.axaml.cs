using AdministratorPanel.UI.ViewModels;
using AdministratorPanel.UI.ViewModels.LogCollector;
using AdministratorPanel.UI.ViewModels.ServerManagement;
using AdministratorPanel.UI.Views.LogCollector;
using AdministratorPanel.UI.Views.ServerManagement;
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
        ServerManagementViewModel serverManagementViewModel) : this()
    {
        DataContext = viewModel;

        var logCollectorView = this.FindControl<LogCollectorView>("LogCollectorHost");
        if (logCollectorView is not null)
        {
            logCollectorView.DataContext = logCollectorViewModel;
        }

        var serverManagementView = this.FindControl<ServerManagementView>("ServerManagementHost");
        if (serverManagementView is not null)
        {
            serverManagementView.DataContext = serverManagementViewModel;
        }
    }
}