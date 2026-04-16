using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdministratorPanel.Core.Entities;

namespace AdministratorPanel.Core.Abstractions
{
    public interface IToolModule // контракт, необходимо реализовать метод GetToolInfo
    {
        string ModuleKey { get; } // каждый модуль обязан иметь ключ
        AdminTool GetToolInfo(); // иии информацию о себе
    }
}
