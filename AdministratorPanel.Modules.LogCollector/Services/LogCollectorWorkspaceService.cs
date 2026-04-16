using AdministratorPanel.Modules.LogCollector.Abstractions;
using AdministratorPanel.Modules.LogCollector.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdministratorPanel.Modules.LogCollector.Services
{
    public sealed class LogCollectorWorkspaceService : ILogCollectorWorkspaceService
    {
        private readonly IServerGroupRepository _serverGroupRepository;

        public LogCollectorWorkspaceService(IServerGroupRepository serverGroupRepository)
        {
            _serverGroupRepository = serverGroupRepository;
        }

        public IReadOnlyCollection<ServerGroupDto> GetServerGroups()
        {
            return _serverGroupRepository.GetAll()
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.Name)
                .Select(group => new ServerGroupDto
                {
                    Id = group.Id,
                    Name = group.Name,
                    Description = group.Description,
                    SortOrder = group.SortOrder,
                    Servers = group.Servers
                        .OrderBy(s => s.SortOrder)
                        .ThenBy(s => s.DisplayName)
                        .Select(server => new RemoteServerDto
                        {
                            Id = server.Id,
                            IpAddress = server.IpAddress,
                            DisplayName = server.DisplayName,
                            IsEnabled = server.IsEnabled,
                            SortOrder = server.SortOrder
                        })
                        .ToArray()
                })
                .ToArray();
        }
    }
}
