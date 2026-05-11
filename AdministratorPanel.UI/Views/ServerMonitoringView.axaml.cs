using AdministratorPanel.UI.ViewModels.ServerMonitoring;
using Avalonia.Controls;
using System;

namespace AdministratorPanel.UI.Views.ServerMonitoring;

public partial class ServerMonitoringView : UserControl
{
    public ServerMonitoringView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private async void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is ServerMonitoringViewModel vm)
        {
            DataContextChanged -= OnDataContextChanged; // чтобы не дёргалось повторно
            await vm.InitializeAsync();
        }
    }
}