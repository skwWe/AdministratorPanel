using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdministratorPanel.Modules.LogCollector.Models;

namespace AdministratorPanel.Modules.LogCollector.Abstractions
{
    public interface ILogCollectorWorkspaceService
    {
        IReadOnlyCollection<ServerGroupDto> GetServerGroups();
    }
}
