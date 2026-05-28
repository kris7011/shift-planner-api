using ShiftPlanner.Domain.Shifts;

namespace ShiftPlanner.Domain.Employees.Preferences;

public class EmployeePreferenceProfile
{
    public Guid EmployeeId { get; set; }

    public List<ShiftType> PreferredShiftTypes { get; set; } = new();

    public List<ShiftType> DislikedShiftTypes { get; set; } = new();

    public int? MaxNightShifts { get; set; }

    public int? MaxEveningShifts { get; set; }

    public bool PrefersWeekends { get; set; }

    public bool AvoidsWeekends { get; set; }
}