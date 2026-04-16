using AdministratorPanel.Modules.LogCollector.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdministratorPanel.Modules.LogCollector.Abstractions
{
    public interface IServerGroupRepository
    {
        IReadOnlyCollection<ServerGroup> GetAll();

        ServerGroup? GetByName(string groupName);
    }
}
