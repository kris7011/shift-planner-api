using ShiftPlanner.Domain.Employees;

namespace ShiftPlanner.Application.Employees.Preferences;

public class EmployeePreferenceProfileOverviewService
{
    private readonly EmployeePreferenceProfileProvider _profileProvider;

    public EmployeePreferenceProfileOverviewService(
        EmployeePreferenceProfileProvider profileProvider)
    {
        _profileProvider = profileProvider;
    }

    public List<EmployeePreferenceProfileOverviewItem> CreateOverview(
        List<Employee> employees)
    {
        return employees
            .Select(employee =>
            {
                var profile = _profileProvider.GetProfile(employee);

                return new EmployeePreferenceProfileOverviewItem
                {
                    EmployeeId = employee.Id,
                    EmployeeName = employee.Name,
                    PreferredShiftTypes = profile.PreferredShiftTypes,
                    DislikedShiftTypes = profile.DislikedShiftTypes,
                    MaxNightShifts = profile.MaxNightShifts,
                    MaxEveningShifts = profile.MaxEveningShifts,
                    PrefersWeekends = profile.PrefersWeekends,
                    AvoidsWeekends = profile.AvoidsWeekends
                };
            })
            .OrderBy(item => item.EmployeeName)
            .ToList();
    }
}