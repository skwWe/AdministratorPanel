using AdministratorPanel.Core.Enums;

namespace AdministratorPanel.Core.Entities;

public sealed class AdminTool : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string IconKey { get; set; } = string.Empty;

    public ToolType Type { get; set; }

    public ToolAvailabilityStatus AvailabilityStatus { get; set; } = ToolAvailabilityStatus.Available;

    public bool IsEnabled { get; set; } = true;

    public int SortOrder { get; set; }

    public Guid? CategoryId { get; set; }
}   