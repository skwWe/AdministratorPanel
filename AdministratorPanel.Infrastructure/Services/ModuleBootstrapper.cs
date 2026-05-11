using AdministratorPanel.Application.Abstractions;
using AdministratorPanel.Core.Abstractions;
using AdministratorPanel.Modules.LogCollector.Modules;
using AdministratorPanel.Modules.ServerManagement.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdministratorPanel.Modules.ServerMonitoring.Modules;


namespace AdministratorPanel.Infrastructure.Services
{
    public sealed class ModuleBootstrapper
    {
        private readonly IToolRegistry _toolRegistry;

        public ModuleBootstrapper(IToolRegistry toolRegistry)
        {
            _toolRegistry = toolRegistry;
        }

        public void RegisterModules()
        {
            Register(new LogCollectorModule());
            Register(new ServerManagementModule());
            Register(new ServerMonitoringModule());
        }

        private void Register(IToolModule module)
        {
            _toolRegistry.RegisterModule(module);
        }
    }
}
