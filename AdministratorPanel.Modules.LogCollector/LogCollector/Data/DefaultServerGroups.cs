using AdministratorPanel.Modules.LogCollector.Entities;

namespace AdministratorPanel.Modules.LogCollector.Data;

public static class DefaultServerGroups
{
    public static IReadOnlyCollection<ServerGroup> Create()
    {
        var groups = new List<ServerGroup>();

        // --- Существующие группы (App, Convert, Sync, Web) ---
        groups.Add(CreateGroup("App", "Основная группа серверов приложения.", 1, new[]
        {
        ("nn-lsed-app12", "10.10.130.110"),
        ("nn-lsed-app02", "10.10.130.84"),
        ("nn-lsed-app13", "10.10.130.111"),
        ("nn-lsed-app14", "10.10.130.112"),
        ("nn-lsed-app11", "10.10.130.109"),
        ("nn-lsed-app06", "10.10.130.235"),
        ("nn-lsed-app07", "10.10.130.236"),
        ("nn-lsed-app08", "10.10.130.237"),
        ("nn-lsed-app09", "10.10.130.238"),
        ("nn-lsed-app10", "10.10.130.177"),
        ("nn-lsed-app-t", "10.10.130.189")
    }));

        groups.Add(CreateGroup("Convert", "Группа серверов конвертации.", 2, new[]
        {
        ("nn-lsed-convert01", "10.10.130.108"),
        ("nn-lsed-convert02", "10.10.130.58"),
        ("nn-lsed-convert03", "10.10.130.59"),
        ("nn-lsed-convert04", "10.10.130.60"),
        ("nn-lsed-convert05", "10.10.130.61")
    }));

        groups.Add(CreateGroup("Sync", "Группа серверов синхронизации.", 3, new[]
        {
        ("nn-lsed-sync01", "10.10.130.70"),
        ("nn-lsed-sync02", "10.10.130.62"),
        ("nn-lsed-sync03", "10.10.130.64"),
        ("nn-lsed-sync04", "10.10.130.65"),
        ("nn-lsed-sync05", "10.10.130.66"),
        ("nn-lsed-sync06", "10.10.130.67"),
        ("nn-lsed-sync07", "10.10.130.68"),
        ("nn-lsed-sync08", "10.10.130.69"),
        ("nn-lsed-sync-t", "10.10.130.99")
    }));

        groups.Add(CreateGroup("Web", "Группа web-серверов.", 4, new[]
        {
        ("nn-lsed-web01", "10.10.130.85"),
        ("nn-lsed-web17", "10.10.130.173"),
        ("nn-lsed-web18", "10.10.130.174"),
        ("nn-lsed-web20", "10.10.130.169"),
        ("nn-lsed-web21", "10.10.130.166"),
        ("nn-lsed-web22", "10.10.130.165"),
        ("nn-lsed-web16", "10.10.130.172"),
        ("nn-lsed-web08", "10.10.130.247"),
        ("nn-lsed-web09", "10.10.130.248"),
        ("nn-lsed-web10", "10.10.130.249"),
        ("nn-lsed-web11", "10.10.130.250"),
        ("nn-lsed-web12", "10.10.130.251"),
        ("nn-lsed-web13", "10.10.130.252"),
        ("nn-lsed-web14", "10.10.130.253"),
        ("nn-lsed-web15", "10.10.130.254"),
        ("nn-lsed-web-t", "10.10.130.190")
    }));

        // --- Новые группы ---
        groups.Add(CreateGroup("Dev", "Серверы разработки.", 5, new[] { ("nn-lsed-sc", "10.10.130.6") }));
        groups.Add(CreateGroup("Intgr", "Интеграционные серверы.", 6, new[] { ("nn-lsed-intgr-t", "10.10.130.90") }));
        groups.Add(CreateGroup("Medo", "Серверы medo.", 7, new[] { ("nn-lsed-medo", "10.10.130.21") }));
        groups.Add(CreateGroup("Sstu", "Серверы sstu.", 8, new[] { ("nn-lsed-sstu", "10.10.130.22") }));
        groups.Add(CreateGroup("Monit", "Мониторинг.", 9, new[] { ("nn-lsed-monit", "10.10.130.107") }));
        groups.Add(CreateGroup("Mgmt", "Управление.", 10, new[]
        {
        ("nn-lsed-mgmt", "10.10.130.103"),
        ("nn-lsed-mgmt-t", "10.10.130.100")
    }));
        groups.Add(CreateGroup("Crldmz", "DMZ CRL.", 11, new[]
        {
        ("nn-lsed-crldmz01", "10.10.130.106"),
        ("nn-lsed-crldmz02", "10.10.130.79"),
        ("nn-lsed-clrdmz-t", "10.10.130.191")
    }));
        groups.Add(CreateGroup("Search", "Поисковые серверы.", 12, new[]
        {
        ("nn-lsed-dirsearch01", "10.10.130.71"),
        ("nn-lsed-dirsearch02", "10.10.130.72"),
        ("nn-lsed-search01", "10.10.130.73"),
        ("nn-lsed-search02", "10.10.130.74"),
        ("nn-lsed-search03", "10.10.130.75"),
        ("nn-lsed-search-t", "10.10.130.92")
    }));
        groups.Add(CreateGroup("Service", "Служебные серверы.", 13, new[]
        {
        ("nn-lsed-service01", "10.10.130.104"),
        ("nn-lsed-service02", "10.10.130.20"),
        ("nn-lsed-service-t", "10.10.130.188")
    }));
        groups.Add(CreateGroup("Dmz", "DMZ общая.", 14, new[]
        {
        ("nn-lsed-dmz01", "10.10.130.105"),
        ("nn-lsed-dmz02", "10.10.130.81"),
        ("nn-lsed-dmz-t", "10.10.130.98")
    }));
        groups.Add(CreateGroup("Office", "Офисные серверы.", 15, new[]
        {
        ("nn-lsed-office01", "10.10.130.80"),
        ("nn-lsed-office02", "10.10.130.170")
    }));
        groups.Add(CreateGroup("Bl2", "Серверы балансировки (2).", 16, new[]
        {
        ("nn-lsed-bl01", "10.10.130.178"),
        ("nn-lsed-bl02_new", "10.10.130.171"),
        ("nn-lsed-bl-t", "10.10.130.97")
    }));
        groups.Add(CreateGroup("Bl1", "Серверы балансировки (1).", 17, new[]
        {
        ("nn-lsed-bl03", "10.10.130.82"),
        ("nn-lsed-bl04", "10.10.130.83")
    }));
        groups.Add(CreateGroup("Redis", "Серверы Redis.", 18, new[]
        {
        ("nn-lsed-redis01", "10.10.130.76"),
        ("nn-lsed-redis02", "10.10.130.86"),
        ("nn-lsed-redis03", "10.10.130.87"),
        ("nn-lsed-redis04", "10.10.130.88"),
        ("nn-lsed-redis05", "10.10.130.77"),
        ("nn-lsed-redis06", "10.10.130.78")
    }));
        groups.Add(CreateGroup("Files", "Файловый сервер.", 19, new[] { ("nn-sed-files", "10.10.130.96") }));
        groups.Add(CreateGroup("Dc", "Доменные контроллеры.", 20, new[]
        {
        ("nn-lsed-dc12", "10.10.130.175"),
        ("nn-lsed-dc13", "10.10.130.176"),
        ("nn-lsed-dc14", "10.10.130.179"),
        ("nn-lsed-dc16", "10.10.130.181"),
        ("nn-lsed-dc15", "10.10.130.180"),
        ("nn-lsed-dc18", "10.10.130.183"),
        ("nn-lsed-dc19", "10.10.130.184"),
        ("nn-lsed-dc17", "10.10.130.182")
    }));
        groups.Add(CreateGroup("Backup", "Сервер резервного копирования.", 21, new[] { ("nn-lsed-backup", "10.10.130.113") }));
        groups.Add(CreateGroup("Dbpg", "Базы данных PostgreSQL.", 22, new[]
        {
        ("nn-sed-dbpg1", "10.10.130.9"),
        ("nn-sed-dbpg2", "10.10.130.8"),
        ("nn-lsed-test-db1", "10.10.130.102")
    }));
        groups.Add(CreateGroup("Quor", "Кворум PG.", 23, new[] { ("nn-nsed-pgquor", "10.10.130.233") }));
        groups.Add(CreateGroup("Pgprx", "Прокси PG.", 24, new[]
        {
        ("nn-nsed-pgprx01", "10.10.130.228"),
        ("nn-nsed-pgprx02", "10.10.130.232")
    }));

        return groups;
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