using ShiftPlanner.Domain.Shifts;

namespace ShiftPlanner.Application.Employees;

public class EmployeeAssignedShiftLoadItem
{
    public Guid ShiftId { get; set; }

    public DateOnly Date { get; set; }

    public ShiftType ShiftType { get; set; }

    public string RequiredSkill { get; set; } = string.Empty;

    public int LoadScore { get; set; }
}