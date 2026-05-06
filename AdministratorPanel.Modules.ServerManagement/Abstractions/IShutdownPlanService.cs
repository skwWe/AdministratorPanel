using AdministratorPanel.Modules.ServerManagement.Models.ShutdownPlans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdministratorPanel.Modules.ServerManagement.Abstractions
{
    public interface IShutdownPlanService
    {
        Task<ShutdownPlanExecutionResultDto> ExecutePlanAsync(
            ShutdownPlanDto plan,
            string sshUserName,
            string password,
            IProgress<string>? progress = null,
            CancellationToken cancellationToken = default);

        Task<ShutdownPlanExecutionResultDto> ExecuteGroupAsync(
            ShutdownPlanGroupDto group,
            string sshUserName,
            string password,
            IProgress<string>? progress = null,
            CancellationToken cancellationToken = default);
    }
}
