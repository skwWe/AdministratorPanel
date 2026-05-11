using AdministratorPanel.Application.Abstractions;
using AdministratorPanel.Core.Enums;
using AdministratorPanel.UI.ViewModels.ServerMonitoring;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Linq;

namespace AdministratorPanel.UI.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    private ToolItemViewModel? _selectedTool;

    [ObservableProperty]
    private string _pageTitle = "AdministratorPanel";

    [ObservableProperty]
    private string _pageDescription = "Многофункциональная панель администратора.";

    public ServerMonitoringViewModel? ServerMonitoringVm { get; set; }

    public bool IsServerManagementSelected => SelectedTool?.Tool.Type == ToolType.ServerManagement;
    public bool IsLogCollectorSelected => SelectedTool?.Tool.Type == ToolType.LogCollector;
    public bool IsServerMonitoringSelected => SelectedTool?.Tool.Type == ToolType.ServerMonitoring;

    public MainWindowViewModel(IToolProvider toolProvider)
    {
        Tools = new ObservableCollection<ToolItemViewModel>(
            toolProvider.GetTools().Select(tool => new ToolItemViewModel(tool)));
        SelectedTool = Tools.FirstOrDefault();
        UpdateSelectedToolInfo();
    }

    public ObservableCollection<ToolItemViewModel> Tools { get; }

    partial void OnSelectedToolChanged(ToolItemViewModel? value)
    {
        UpdateSelectedToolInfo();
        OnPropertyChanged(nameof(IsLogCollectorSelected));
        OnPropertyChanged(nameof(IsServerManagementSelected));
        OnPropertyChanged(nameof(IsServerMonitoringSelected));
    }

    private void UpdateSelectedToolInfo()
    {
        if (SelectedTool is null)
        {
            PageTitle = "AdministratorPanel";
            PageDescription = "Инструменты не найдены.";
            return;
        }
        PageTitle = SelectedTool.Name;
        PageDescription = SelectedTool.Description;
    }
}