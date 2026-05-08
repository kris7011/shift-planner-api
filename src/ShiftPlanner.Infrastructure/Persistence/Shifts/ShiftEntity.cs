using ShiftPlanner.Domain.Shifts;

namespace ShiftPlanner.Infrastructure.Persistence.Shifts;

public class ShiftEntity
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public DateOnly Date { get; set; }
    public ShiftType ShiftType { get; set; }
    public string RequiredSkill { get; set; } = string.Empty;
}