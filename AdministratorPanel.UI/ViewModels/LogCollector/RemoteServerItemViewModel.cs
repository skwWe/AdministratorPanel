using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdministratorPanel.UI.ViewModels.LogCollector
{
    public sealed class RemoteServerItemViewModel : ViewModelBase
    {
        public Guid Id { get; init; }

        public string DisplayName { get; init; } = string.Empty;

        public string IpAddress { get; init; } = string.Empty;

        public bool IsEnabled { get; init; }
    }
}
