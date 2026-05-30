using ShiftPlanner.Domain.Shifts;

namespace ShiftPlanner.Application.Employees.Preferences;

public class EmployeePreferenceProfileOverviewItem
{
    public Guid EmployeeId { get; set; }

    public string EmployeeName { get; set; } = string.Empty;

    public List<ShiftType> PreferredShiftTypes { get; set; } = new();

    public List<ShiftType> DislikedShiftTypes { get; set; } = new();

    public int? MaxNightShifts { get; set; }

    public int? MaxEveningShifts { get; set; }

    public bool PrefersWeekends { get; set; }

    public bool AvoidsWeekends { get; set; }
}