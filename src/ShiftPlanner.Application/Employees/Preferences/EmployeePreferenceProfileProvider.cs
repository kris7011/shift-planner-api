using ShiftPlanner.Domain.Employees;
using ShiftPlanner.Domain.Employees.Preferences;
using ShiftPlanner.Domain.Shifts;

namespace ShiftPlanner.Application.Employees.Preferences;

public class EmployeePreferenceProfileProvider
{
    public EmployeePreferenceProfile GetProfile(Employee employee)
    {
        var profile = new EmployeePreferenceProfile
        {
            EmployeeId = employee.Id
        };

        if (employee.Name == "Henrik")
        {
            profile.PreferredShiftTypes.Add(ShiftType.Night);
            profile.MaxNightShifts = 3;
        }

        if (employee.Name == "Mette")
        {
            profile.DislikedShiftTypes.Add(ShiftType.Night);
            profile.MaxNightShifts = 1;
        }

        if (employee.Name == "Peter")
        {
            profile.AvoidsWeekends = true;
        }

        return profile;
    }
}