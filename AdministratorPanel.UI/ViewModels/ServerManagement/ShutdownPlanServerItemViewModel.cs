using AdministratorPanel.Modules.ServerManagement.Models.ShutdownPlans;

namespace AdministratorPanel.UI.ViewModels.ServerManagement
{
    public sealed class ShutdownPlanServerItemViewModel
    {
        public int Order { get; init; }

        public int RowNumber { get; init; }

        public string GroupName { get; init; } = string.Empty;

        public string IpAddress { get; init; } = string.Empty;

        public string HostName { get; init; } = string.Empty;

        public string DomainName { get; init; } = string.Empty;

        public string DisplayName
        {
            get
            {
                if (string.IsNullOrWhiteSpace(DomainName))
                    return $"{Order}. {HostName} / {IpAddress}";

                return $"{Order}. {HostName} / {DomainName} / {IpAddress}";
            }
        }

        public static ShutdownPlanServerItemViewModel FromDto(ShutdownPlanServerDto server)
        {
            return new ShutdownPlanServerItemViewModel
            {
                Order = server.Order,
                RowNumber = server.RowNumber,
                GroupName = server.GroupName,
                IpAddress = server.IpAddress,
                HostName = server.HostName,
                DomainName = server.DomainName
            };
        }
    }
}