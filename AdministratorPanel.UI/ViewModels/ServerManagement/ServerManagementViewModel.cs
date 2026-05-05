using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdministratorPanel.UI.ViewModels.ServerManagement
{

    public partial class ServerManagementViewModel : ViewModelBase // разделение класса на несколько частей
    {
        [ObservableProperty] // Код генерируется автоматически при помощи CommunityToolkit.Mvvm.ComponentModel
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
    }
}
