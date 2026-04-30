using AdministratorPanel.Modules.LogCollector.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdministratorPanel.Modules.LogCollector.Abstractions
{
    public interface ISshCommandRunner
    {
        Task<SshCommandResult> RunAsync(
            string host,
            string username,
            string password,
            string command,
            CancellationToken cancellationToken = default);
    }
}
