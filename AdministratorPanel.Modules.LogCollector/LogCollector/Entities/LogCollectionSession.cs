using AdministratorPanel.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdministratorPanel.Modules.LogCollector.Entities
{
    public sealed class LogCollectionSession : BaseEntity
    {
        public DateTime StartedAtUtc { get; set; }

        public DateTime? FinishedAtUtc { get; set; }

        public string InitiatedBy { get; set; } = string.Empty;

        public string TargetName { get; set; } = string.Empty;

        public string Notes { get; set; } = string.Empty;
    }
}
