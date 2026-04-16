using AdministratorPanel.Modules.LogCollector.Enums;
using AdministratorPanel.Modules.LogCollector.Models;

namespace AdministratorPanel.Modules.LogCollector.Abstractions;

public interface ILogScriptRunner
{
    Task<LogScriptExecutionResult> RunAsync(
        LogCollectionGroupType groupType,
        string sshUserName,
        string? password,
        IProgress<LogScriptProgressMessage>? progress = null,
        CancellationToken cancellationToken = default);
}