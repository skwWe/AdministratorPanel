using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdministratorPanel.Core.Entities
{
    public sealed class NavigationItem : BaseEntity
    {
        public string Title { get; set; } = string.Empty;

        public string IconKey { get; set; } = string.Empty;

        public string Route { get; set; } = string.Empty;

        public bool IsVisible { get; set; } = true;

        public int SortOrder { get; set; }

        public Guid? ToolId { get; set; }
    }
}
