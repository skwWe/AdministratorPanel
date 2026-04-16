using AdministratorPanel.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdministratorPanel.UI.ViewModels
{
    public sealed partial class ToolItemViewModel : ViewModelBase
    {
        public ToolItemViewModel(AdminTool tool)
        {
            Tool = tool;
        }

        public AdminTool Tool { get; }

        public string Name => Tool.Name;

        public string Description => Tool.Description;

        public string IconKey => Tool.IconKey;
    }
}
