using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdministratorPanel.Modules.LogCollector.Enums
{
    public enum ServerCollectionStatus
    {
        Unknown = 0,
        Pending = 1,
        Running = 2,
        Success = 3,
        NoDocker = 4,
        NoContainers = 5,
        SshError = 6,
        Failed = 7
    }
}
