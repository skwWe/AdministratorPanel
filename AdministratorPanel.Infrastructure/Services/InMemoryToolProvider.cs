using AdministratorPanel.Application.Abstractions;
using AdministratorPanel.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdministratorPanel.Infrastructure.Services
{
    public sealed class InMemoryToolProvider : IToolProvider
    {
        private readonly IToolRegistry _toolRegistry;

        public InMemoryToolProvider(IToolRegistry toolRegistry)
        {
            _toolRegistry = toolRegistry;
        }

        public IReadOnlyCollection<AdminTool> GetTools()
        {
            return _toolRegistry.GetEnabledTools();
        }
    }
}
