using ShiftPlanner.Application.Employees.Preferences;
using ShiftPlanner.Domain.Employees;
using ShiftPlanner.Domain.Shifts;

namespace ShiftPlanner.Tests.Application;

public class EmployeePreferenceProfileProviderTests
{
    [Fact]
    public void GetProfile_ReturnsNightPreference_ForHenrik()
    {
        var employee = new Employee("Henrik", new List<string> { "Night" });
        var provider = new EmployeePreferenceProfileProvider();

        var profile = provider.GetProfile(employee);

        Assert.Equal(employee.Id, profile.EmployeeId);
        Assert.Contains(ShiftType.Night, profile.PreferredShiftTypes);
        Assert.Equal(3, profile.MaxNightShifts);
    }

    [Fact]
    public void GetProfile_ReturnsNightDislike_ForMette()
    {
        var employee = new Employee("Mette", new List<string> { "MRI" });
        var provider = new EmployeePreferenceProfileProvider();

        var profile = provider.GetProfile(employee);

        Assert.Equal(employee.Id, profile.EmployeeId);
        Assert.Contains(ShiftType.Night, profile.DislikedShiftTypes);
        Assert.Equal(1, profile.MaxNightShifts);
    }

    [Fact]
    public void GetProfile_ReturnsWeekendAvoidance_ForPeter()
    {
        var employee = new Employee("Peter", new List<string> { "CT" });
        var provider = new EmployeePreferenceProfileProvider();

        var profile = provider.GetProfile(employee);

        Assert.Equal(employee.Id, profile.EmployeeId);
        Assert.True(profile.AvoidsWeekends);
    }

    [Fact]
    public void GetProfile_ReturnsNeutralProfile_ForUnknownEmployee()
    {
        var employee = new Employee("Anna", new List<string> { "XR" });
        var provider = new EmployeePreferenceProfileProvider();

        var profile = provider.GetProfile(employee);

        Assert.Equal(employee.Id, profile.EmployeeId);
        Assert.Empty(profile.PreferredShiftTypes);
        Assert.Empty(profile.DislikedShiftTypes);
        Assert.Null(profile.MaxNightShifts);
        Assert.Null(profile.MaxEveningShifts);
        Assert.False(profile.PrefersWeekends);
        Assert.False(profile.AvoidsWeekends);
    }
}