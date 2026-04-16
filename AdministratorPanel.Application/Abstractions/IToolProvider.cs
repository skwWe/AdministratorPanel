using AdministratorPanel.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdministratorPanel.Application.Abstractions
{
    public interface IToolProvider
    {
        IReadOnlyCollection<AdminTool> GetTools();
    }
}
