using ShiftPlanner.Application.Employees.Preferences;
using ShiftPlanner.Domain.Employees;
using ShiftPlanner.Domain.Shifts;

namespace ShiftPlanner.Tests.Application;

public class EmployeePreferenceProfileOverviewServiceTests
{
    [Fact]
    public void CreateOverview_ReturnsPreferenceProfilesOrderedByEmployeeName()
    {
        var employees = new List<Employee>
        {
            new("Mette", new List<string> { "MRI" }),
            new("Henrik", new List<string> { "Night" })
        };

        var provider = new EmployeePreferenceProfileProvider();
        var service = new EmployeePreferenceProfileOverviewService(provider);

        var result = service.CreateOverview(employees);

        Assert.Equal(2, result.Count);

        Assert.Equal("Henrik", result[0].EmployeeName);
        Assert.Contains(ShiftType.Night, result[0].PreferredShiftTypes);
        Assert.Equal(3, result[0].MaxNightShifts);

        Assert.Equal("Mette", result[1].EmployeeName);
        Assert.Contains(ShiftType.Night, result[1].DislikedShiftTypes);
        Assert.Equal(1, result[1].MaxNightShifts);
    }
}