using ShiftPlanner.Domain.Shifts;

namespace ShiftPlanner.Infrastructure.Persistence.Shifts;

public class ShiftEntity
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public DateOnly Date { get; set; }
    public ShiftType ShiftType { get; set; }
    public string RequiredSkill { get; set; } = string.Empty;

    /// <summary>Open shifts are stored with <see cref="Guid.Empty"/> as the employee id.</summary>
    public static ShiftEntity FromDomain(Shift shift)
    {
        return new ShiftEntity
        {
            Id = shift.Id,
            EmployeeId = shift.EmployeeId ?? Guid.Empty,
            Date = shift.Date,
            ShiftType = shift.ShiftType,
            RequiredSkill = shift.RequiredSkill
        };
    }
}