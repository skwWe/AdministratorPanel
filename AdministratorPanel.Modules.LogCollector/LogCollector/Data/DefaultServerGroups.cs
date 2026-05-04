using AdministratorPanel.Modules.LogCollector.Entities;

namespace AdministratorPanel.Modules.LogCollector.Data;

public static class DefaultServerGroups
{
    public static IReadOnlyCollection<ServerGroup> Create()
    {
        var appGroup = CreateGroup(
            name: "App",
            description: "Основная группа серверов приложения.",
            sortOrder: 1,
            servers:
            [
                ("nn-lsed-app01", "10.10.130.110"),
                ("nn-lsed-app02", "10.10.130.111"),
                ("nn-lsed-app03", "10.10.130.112"),
                ("nn-lsed-app04", "10.10.130.109"),
                ("nn-lsed-app05", "10.10.130.235"),
                ("nn-lsed-app06", "10.10.130.236"),
                ("nn-lsed-app07", "10.10.130.237"),
                ("nn-lsed-app08", "10.10.130.238"),
                ("nn-lsed-app09", "10.10.130.177")
            ]);

        var convertGroup = CreateGroup(
            name: "Convert",
            description: "Группа серверов сервиса конвертации.",
            sortOrder: 2,
            servers:
            [
                ("nn-lsed-convert01", "10.10.130.108"),
                ("nn-lsed-convert02", "10.10.130.58"),
                ("nn-lsed-convert03", "10.10.130.59"),
                ("nn-lsed-convert04", "10.10.130.60"),
                ("nn-lsed-convert05", "10.10.130.61")
            ]);

        var syncGroup = CreateGroup(
            name: "Sync",
            description: "Группа серверов синхронизации.",
            sortOrder: 3,
            servers:
            [
                ("nn-lsed-sync01", "10.10.130.62"),
                ("nn-lsed-sync02", "10.10.130.64"),
                ("nn-lsed-sync03", "10.10.130.65"),
                ("nn-lsed-sync04", "10.10.130.66"),
                ("nn-lsed-sync05", "10.10.130.67"),
                ("nn-lsed-sync06", "10.10.130.68"),
                ("nn-lsed-sync07", "10.10.130.69")
            ]);

        var webGroup = CreateGroup(
            name: "Web",
            description: "Группа web-серверов.",
            sortOrder: 4,
            servers:
            [
                ("nn-lsed-web01", "10.10.130.173"),
                ("nn-lsed-web02", "10.10.130.174"),
                ("nn-lsed-web03", "10.10.130.169"),
                ("nn-lsed-web04", "10.10.130.166"),
                ("nn-lsed-web05", "10.10.130.165"),
                ("nn-lsed-web06", "10.10.130.172"),
                ("nn-lsed-web07", "10.10.130.247"),
                ("nn-lsed-web08", "10.10.130.248"),
                ("nn-lsed-web09", "10.10.130.249"),
                ("nn-lsed-web10", "10.10.130.250"),
                ("nn-lsed-web11", "10.10.130.251"),
                ("nn-lsed-web12", "10.10.130.252"),
                ("nn-lsed-web13", "10.10.130.253"),
                ("nn-lsed-web14", "10.10.130.254")
            ]);

        return
        [
            appGroup,
            convertGroup,
            syncGroup,
            webGroup
        ];
    }

    private static ServerGroup CreateGroup(
        string name,
        string description,
        int sortOrder,
        IReadOnlyList<(string HostName, string IpAddress)> servers)
    {
        var group = new ServerGroup
        {
            Name = name,
            Description = description,
            SortOrder = sortOrder
        };

        for (var index = 0; index < servers.Count; index++)
        {
            var server = servers[index];

            group.Servers.Add(new RemoteServer
            {
                DisplayName = server.HostName,
                IpAddress = server.IpAddress,
                SortOrder = index + 1,
                ServerGroupId = group.Id,
                IsEnabled = true
            });
        }

        return group;
    }
}