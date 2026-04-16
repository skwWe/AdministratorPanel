using AdministratorPanel.Application.Abstractions;
using AdministratorPanel.Application.Services;
using AdministratorPanel.Infrastructure.LogCollector.Repositories;
using AdministratorPanel.Infrastructure.LogCollector.Services;
using AdministratorPanel.Infrastructure.Services;
using AdministratorPanel.Modules.LogCollector.Abstractions;
using AdministratorPanel.Modules.LogCollector.Services;
using Microsoft.Extensions.DependencyInjection;

namespace AdministratorPanel.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAdministratorPanelInfrastructure(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<IToolRegistry, ToolRegistry>();
        services.AddSingleton<IToolProvider, InMemoryToolProvider>();

        services.AddSingleton<IServerGroupRepository, DefaultServerGroupRepository>();
        services.AddSingleton<ILogCollectorWorkspaceService, LogCollectorWorkspaceService>();
        services.AddSingleton<ILogScriptRunner, BashLogScriptRunner>();

        services.AddSingleton<ModuleBootstrapper>();

        return services;
    }
}