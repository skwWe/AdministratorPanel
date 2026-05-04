using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdministratorPanel.Modules.ServerManagement.Models.Operations
{
    public sealed class ServerOperationResultDto
    {
        public ServerOperationStatus Status { get; init; }

        public string ServerName { get; init; } = string.Empty;

        public string IpAddress { get; init; } = string.Empty;

        public string OperationName { get; init; } = string.Empty;

        public string Output { get; init; } = string.Empty;

        public string Error { get; init; } = string.Empty;

        public DateTime ExecutedAt { get; init; } = DateTime.Now;

        public bool IsSuccess => Status == ServerOperationStatus.Success;
    }
}