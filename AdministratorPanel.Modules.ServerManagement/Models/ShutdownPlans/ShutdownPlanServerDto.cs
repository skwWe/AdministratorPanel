using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdministratorPanel.Modules.ServerManagement.Models.ShutdownPlans
{
    public sealed class ShutdownPlanServerDto
    {
        public int Order { get; init; }

        public int RowNumber { get; init; }

        public string GroupName { get; init; } = string.Empty;

        public string IpAddress { get; init; } = string.Empty;

        public string HostName { get; init; } = string.Empty;

        public string DomainName { get; init; } = string.Empty;

        public string DisplayName =>
            string.IsNullOrWhiteSpace(DomainName)
                ? $"{HostName} / {IpAddress}"
                : $"{HostName} / {DomainName} / {IpAddress}";
    }
}
