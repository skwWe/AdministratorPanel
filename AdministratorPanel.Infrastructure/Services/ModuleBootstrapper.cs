using AdministratorPanel.Application.Abstractions;
using AdministratorPanel.Core.Abstractions;
using AdministratorPanel.Modules.LogCollector.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        }

        private void Register(IToolModule module)
        {
            _toolRegistry.RegisterModule(module);
        }
    }
}
