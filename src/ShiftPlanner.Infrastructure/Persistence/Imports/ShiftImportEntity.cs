using ShiftPlanner.Application.Imports;

namespace ShiftPlanner.Infrastructure.Persistence.Imports;

public class ShiftImportEntity
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string ContentHash { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public int MaxAssignmentsPerEmployee { get; set; }
    public ShiftImportStatus Status { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? ConfirmedAt { get; set; }
    public int CreatedShiftCount { get; set; }
}
