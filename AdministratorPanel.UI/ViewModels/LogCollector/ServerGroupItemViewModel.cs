using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdministratorPanel.UI.ViewModels.LogCollector
{
    public sealed class ServerGroupItemViewModel : ViewModelBase
    {
        public Guid Id { get; init; }

        public string Name { get; init; } = string.Empty;

        public string Description { get; init; } = string.Empty;

        public ObservableCollection<RemoteServerItemViewModel> Servers { get; init; } = new();

        public int ServerCount => Servers.Count;
    }
}
