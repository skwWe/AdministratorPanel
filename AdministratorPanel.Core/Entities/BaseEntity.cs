using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdministratorPanel.Core.Entities
{
    public abstract class BaseEntity // для наследования
    {
        public Guid Id { get; set; } = Guid.NewGuid();
    }
}
