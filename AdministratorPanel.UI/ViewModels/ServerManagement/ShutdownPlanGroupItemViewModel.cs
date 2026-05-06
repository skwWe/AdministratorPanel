using AdministratorPanel.Modules.ServerManagement.Models.ShutdownPlans;
using System.Collections.ObjectModel;
using System.Linq;

namespace AdministratorPanel.UI.ViewModels.ServerManagement
{
    public sealed class ShutdownPlanGroupItemViewModel
    {
        public int Order { get; init; }

        public string Name { get; init; } = string.Empty;

        public ObservableCollection<ShutdownPlanServerItemViewModel> Servers { get; init; } = new();

        public string DisplayName => $"{Order}. {Name} — серверов: {Servers.Count}";

        public static ShutdownPlanGroupItemViewModel FromDto(ShutdownPlanGroupDto group)
        {
            return new ShutdownPlanGroupItemViewModel
            {
                Order = group.Order,
                Name = group.Name,
                Servers = new ObservableCollection<ShutdownPlanServerItemViewModel>(
                    group.Servers.Select(ShutdownPlanServerItemViewModel.FromDto))
            };
        }
    }
}