using AdministratorPanel.Modules.LogCollector.Abstractions;
using AdministratorPanel.Modules.LogCollector.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AdministratorPanel.Modules.LogCollector.Services;

namespace AdministratorPanel.Tests.Modules.LogCollector;

[TestClass]
public class LogCollectorWorkspaceServiceTests
{
    [TestMethod]
    public void GetServerGroups_Should_Map_Data_And_Sort_Groups_And_Servers()
    {
        var repository = new FakeServerGroupRepository(
        [
            new ServerGroup
            {
                Id = Guid.NewGuid(),
                Name = "Zeta",
                Description = "Вторая группа",
                SortOrder = 2,
                Servers =
                [
                    new RemoteServer
                    {
                        Id = Guid.NewGuid(),
                        DisplayName = "Server C",
                        IpAddress = "10.0.0.3",
                        IsEnabled = false,
                        SortOrder = 2
                    },
                    new RemoteServer
                    {
                        Id = Guid.NewGuid(),
                        DisplayName = "Server A",
                        IpAddress = "10.0.0.1",
                        IsEnabled = true,
                        SortOrder = 1
                    }
                ]
            },
            new ServerGroup
            {
                Id = Guid.NewGuid(),
                Name = "Alpha",
                Description = "First group",
                SortOrder = 1,
                Servers =
                [
                    new RemoteServer
                    {
                        Id = Guid.NewGuid(),
                        DisplayName = "Server B",
                        IpAddress = "10.0.0.2",
                        IsEnabled = true,
                        SortOrder = 3
                    }
                ]
            }
        ]);

        var service = new LogCollectorWorkspaceService(repository);

        var groups = service.GetServerGroups().ToArray();

        Assert.AreEqual(2, groups.Length);
        Assert.AreEqual("Alpha", groups[0].Name);
        Assert.AreEqual("Zeta", groups[1].Name);

        Assert.AreEqual("Вторая группа", groups[1].Description);
        Assert.AreEqual(2, groups[1].Servers.Count);

        var zetaServers = groups[1].Servers.ToArray();
        Assert.AreEqual("Server A", zetaServers[0].DisplayName);
        Assert.AreEqual("10.0.0.1", zetaServers[0].IpAddress);
        Assert.IsTrue(zetaServers[0].IsEnabled);

        Assert.AreEqual("Server C", zetaServers[1].DisplayName);
        Assert.IsFalse(zetaServers[1].IsEnabled);
    }

    [TestMethod]
    public void GetServerGroups_Should_Return_Empty_Collection_When_Repository_Is_Empty()
    {
        var service = new LogCollectorWorkspaceService(new FakeServerGroupRepository([]));

        var groups = service.GetServerGroups();

        Assert.IsNotNull(groups);
        Assert.AreEqual(0, groups.Count);
    }

    [TestMethod]
    public void GetServerGroups_Should_Create_New_Dto_Instances()
    {
        var groupId = Guid.NewGuid();
        var serverId = Guid.NewGuid();

        var repository = new FakeServerGroupRepository(
        [
            new ServerGroup
            {
                Id = groupId,
                Name = "App",
                Description = "Application servers",
                SortOrder = 1,
                Servers =
                [
                    new RemoteServer
                    {
                        Id = serverId,
                        DisplayName = "App-01",
                        IpAddress = "192.168.0.10",
                        IsEnabled = true,
                        SortOrder = 1
                    }
                ]
            }
        ]);

        var service = new LogCollectorWorkspaceService(repository);

        var result = service.GetServerGroups().Single();
        var server = result.Servers.Single();

        Assert.AreEqual(groupId, result.Id);
        Assert.AreEqual("App", result.Name);
        Assert.AreEqual("Application servers", result.Description);
        Assert.AreEqual(1, result.SortOrder);

        Assert.AreEqual(serverId, server.Id);
        Assert.AreEqual("App-01", server.DisplayName);
        Assert.AreEqual("192.168.0.10", server.IpAddress);
        Assert.IsTrue(server.IsEnabled);
        Assert.AreEqual(1, server.SortOrder);
    }

    private sealed class FakeServerGroupRepository(IReadOnlyCollection<ServerGroup> groups) : IServerGroupRepository
    {
        public IReadOnlyCollection<ServerGroup> GetAll() => groups;
        public ServerGroup? GetByName(string groupName) => groups.FirstOrDefault(x => x.Name == groupName);
    }
}
