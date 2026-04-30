using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdministratorPanel.UI.ViewModels.LogCollector
{
    public partial class RemoteServerItemViewModel : ObservableObject
    {
        public Guid Id { get; init; }

        [ObservableProperty]
        private string _displayName = string.Empty;

        [ObservableProperty]
        private string _ipAddress = string.Empty;

        [ObservableProperty]
        private bool _isEnabled;
    }
}
