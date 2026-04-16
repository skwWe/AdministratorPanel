using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdministratorPanel.Core.Entities
{
    public sealed class ToolCategory : BaseEntity
    {
        public string Name { get ; set; } = string.Empty;
        public string Description {  get; set; } = string.Empty;
        public int StringOrder { get; set; }
    }
}
