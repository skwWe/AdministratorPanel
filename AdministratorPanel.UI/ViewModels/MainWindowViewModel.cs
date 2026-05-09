using AdministratorPanel.Application.Abstractions;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using AdministratorPanel.Core.Enums;
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

    public bool IsDiagnosticsSelected => SelectedTool?.Tool.Name == "Диагностика логов";
    public bool IsServerManagementSelected =>
    SelectedTool?.Tool.Type == ToolType.ServerManagement;

    public MainWindowViewModel(IToolProvider toolProvider)
    {
        Tools = new ObservableCollection<ToolItemViewModel>(
            toolProvider.GetTools().Select(tool => new ToolItemViewModel(tool)));

        SelectedTool = Tools.FirstOrDefault();
        UpdateSelectedToolInfo();
    }

    public ObservableCollection<ToolItemViewModel> Tools { get; }

    public bool IsLogCollectorSelected =>
        SelectedTool?.Tool.Type == ToolType.LogCollector;

    partial void OnSelectedToolChanged(ToolItemViewModel? value)
    {
        UpdateSelectedToolInfo();

        OnPropertyChanged(nameof(IsLogCollectorSelected));
        OnPropertyChanged(nameof(IsServerManagementSelected));
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