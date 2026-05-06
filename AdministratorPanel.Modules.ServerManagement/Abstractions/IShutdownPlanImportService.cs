using AdministratorPanel.Modules.ServerManagement.Models.ShutdownPlans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdministratorPanel.Modules.ServerManagement.Abstractions
{
    public interface IShutdownPlanImportService
    {
        Task<ShutdownPlanDto> ImportAsync(
            string filePath,
            CancellationToken cancellationToken = default);
    }
}
