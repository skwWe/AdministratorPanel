using AdministratorPanel.Modules.ServerManagement.Models.Operations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdministratorPanel.Modules.ServerManagement.Models.ShutdownPlans
{
    public sealed class ShutdownPlanExecutionResultDto
    {
        public DateTime StartedAt { get; init; }

        public DateTime FinishedAt { get; init; }

        public int TotalServers { get; init; }

        public int SuccessCount { get; init; }

        public int FailedCount { get; init; }

        public IReadOnlyCollection<ServerOperationResultDto> Results { get; init; }
            = Array.Empty<ServerOperationResultDto>();

        public bool IsSuccess => FailedCount == 0;
    }
}
