using AdministratorPanel.Core.Entities;
using AdministratorPanel.Modules.LogCollector.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdministratorPanel.Modules.LogCollector.Entities
{
    public sealed class ServerCollectionResult : BaseEntity
    {
        public Guid SessionId { get; set; }

        public Guid ServerId { get; set; }

        public ServerCollectionStatus Status { get; set; } = ServerCollectionStatus.Unknown;

        public bool DockerAvailable { get; set; }

        public bool ContainersFound { get; set; }

        public string Message { get; set; } = string.Empty;

        public string LogDirectoryPath { get; set; } = string.Empty;

        public string AllLogPath { get; set; } = string.Empty;

        public string InfoFilePath { get; set; } = string.Empty;

        public string DebugFilePath { get; set; } = string.Empty;

        public string ErrorFilePath { get; set; } = string.Empty;
    }
}
