using AdministratorPanel.Modules.ServerManagement.Abstractions;
using AdministratorPanel.Modules.ServerManagement.Models.ShutdownPlans;
using ClosedXML.Excel;
using System.Net;

namespace AdministratorPanel.Infrastructure.ServerManagement.Services
{
    public sealed class ExcelShutdownPlanImportService : IShutdownPlanImportService
    {
        public Task<ShutdownPlanDto> ImportAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            return Task.Run(() =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (string.IsNullOrWhiteSpace(filePath))
                    throw new ArgumentException("Путь к файлу плана не указан.", nameof(filePath));

                if (!File.Exists(filePath))
                    throw new FileNotFoundException("Файл плана отключения не найден.", filePath);

                using var workbook = new XLWorkbook(filePath);
                var worksheet = workbook.Worksheets.First();

                var groups = new List<ShutdownPlanGroupDto>();

                string? currentGroupName = null;
                var currentGroupServers = new List<ShutdownPlanServerDto>();

                var groupOrder = 0;
                var serverOrder = 0;

                var usedRange = worksheet.RangeUsed();

                if (usedRange is null)
                {
                    return new ShutdownPlanDto
                    {
                        Name = Path.GetFileNameWithoutExtension(filePath),
                        SourceFilePath = filePath,
                        LoadedAt = DateTime.Now,
                        Groups = Array.Empty<ShutdownPlanGroupDto>()
                    };
                }

                foreach (var row in usedRange.Rows())
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var rowNumber = row.RowNumber();

                    var columnA = ReadCell(row.Cell(1));
                    var columnB = ReadCell(row.Cell(2));
                    var columnC = ReadCell(row.Cell(3));

                    if (string.IsNullOrWhiteSpace(columnA) &&
                        string.IsNullOrWhiteSpace(columnB) &&
                        string.IsNullOrWhiteSpace(columnC))
                    {
                        continue;
                    }



                    var isGroupRow =
                        !string.IsNullOrWhiteSpace(columnA) &&
                        string.IsNullOrWhiteSpace(columnB) &&
                        string.IsNullOrWhiteSpace(columnC) &&
                        !IsIpAddress(columnA);

                    if (isGroupRow && IsPlanTitleRow(columnA))
                    {
                        continue;
                    }

                        if (isGroupRow)
                    {
                        if (!string.IsNullOrWhiteSpace(currentGroupName))
                        {
                            groups.Add(new ShutdownPlanGroupDto
                            {
                                Order = groupOrder,
                                Name = currentGroupName,
                                Servers = currentGroupServers.ToArray()
                            });
                        }

                        groupOrder++;
                        serverOrder = 0;

                        currentGroupName = NormalizeGroupName(columnA);
                        currentGroupServers = new List<ShutdownPlanServerDto>();

                        continue;
                    }

                    var isServerRow =
                        IsIpAddress(columnA) ||
                        !string.IsNullOrWhiteSpace(columnB) ||
                        !string.IsNullOrWhiteSpace(columnC);

                    if (!isServerRow)
                        continue;

                    if (string.IsNullOrWhiteSpace(currentGroupName))
                    {
                        throw new InvalidOperationException(
                            $"В строке {rowNumber} найден сервер, но перед ним не указана группа.");
                    }

                    if (!IsIpAddress(columnA))
                    {
                        throw new InvalidOperationException(
                            $"В строке {rowNumber} некорректный IP-адрес: '{columnA}'.");
                    }

                    serverOrder++;

                    currentGroupServers.Add(new ShutdownPlanServerDto
                    {
                        Order = serverOrder,
                        RowNumber = rowNumber,
                        GroupName = currentGroupName,
                        IpAddress = columnA,
                        HostName = columnB,
                        DomainName = columnC
                    });
                }

                if (!string.IsNullOrWhiteSpace(currentGroupName))
                {
                    groups.Add(new ShutdownPlanGroupDto
                    {
                        Order = groupOrder,
                        Name = currentGroupName,
                        Servers = currentGroupServers.ToArray()
                    });
                }

                return new ShutdownPlanDto
                {
                    Name = Path.GetFileNameWithoutExtension(filePath),
                    SourceFilePath = filePath,
                    LoadedAt = DateTime.Now,
                    Groups = groups.ToArray()
                };
            }, cancellationToken);
        }

        private static string ReadCell(IXLCell cell)
        {
            return cell.GetString().Trim();
        }

        private static bool IsIpAddress(string value)
        {
            return IPAddress.TryParse(value.Trim(), out _);
        }

        private static string NormalizeGroupName(string value)
        {
            return value.Trim().ToLowerInvariant();
        }

        private static bool IsPlanTitleRow(string value)
        {
            var normalized = value.Trim().ToLowerInvariant();

            return normalized.Contains("план") &&
                   normalized.Contains("выключ");
        }
    }
}