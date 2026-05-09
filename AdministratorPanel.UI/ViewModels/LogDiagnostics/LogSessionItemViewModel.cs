using CommunityToolkit.Mvvm.ComponentModel;
using DocumentFormat.OpenXml.InkML;

namespace AdministratorPanel.UI.ViewModels.LogDiagnostics;

public partial class LogSessionItemViewModel : ObservableObject
{
    [ObservableProperty]
    private string _groupName = string.Empty;

    [ObservableProperty]
    private string _timestamp = string.Empty;

    [ObservableProperty]
    private string _fullPath = string.Empty;

    public string DisplayName => $"{GroupName} / {Timestamp}";
}

public partial class LogServerItemViewModel : ObservableObject
{
    [ObservableProperty]
    private string _ipAddress = string.Empty;

    [ObservableProperty]
    private string _fullPath = string.Empty;

    [ObservableProperty]
    private bool _hasAllLog;

    public string DisplayName => $"{IpAddress} {(HasAllLog ? "" : "(нет логов)")}";
}