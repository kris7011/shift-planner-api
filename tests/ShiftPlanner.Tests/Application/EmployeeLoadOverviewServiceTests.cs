using ShiftPlanner.Application.Employees;
using ShiftPlanner.Domain.Employees;
using ShiftPlanner.Domain.Shifts;

namespace ShiftPlanner.Tests.Application;

public class EmployeeLoadOverviewServiceTests
{
    [Fact]
    public void CreateOverview_ReturnsEmployeesOrderedByTotalLoadDescending()
    {
        var employeeWithHighLoad = new Employee("Kris", new List<string> { "CT" });
        var employeeWithLowLoad = new Employee("Mette", new List<string> { "XR" });

        var dayShift = new Shift(
            new DateOnly(2026, 5, 11),
            ShiftType.Day,
            "CT",
            1);

        var nightShift = new Shift(
            new DateOnly(2026, 5, 12),
            ShiftType.Night,
            "CT",
            1);

        dayShift.AssignEmployee(employeeWithHighLoad);
        nightShift.AssignEmployee(employeeWithHighLoad);

        var employees = new List<Employee>
        {
            employeeWithLowLoad,
            employeeWithHighLoad
        };

        var shifts = new List<Shift>
        {
            dayShift,
            nightShift
        };

        var loadService = new EmployeeLoadService();
        var statusService = new EmployeeLoadStatusService();
        var overviewService = new EmployeeLoadOverviewService(loadService, statusService);

        var overview = overviewService.CreateOverview(employees, shifts);

        Assert.Equal(2, overview.Count);

        Assert.Equal(employeeWithHighLoad.Id, overview[0].EmployeeId);
        Assert.Equal("Kris", overview[0].EmployeeName);
        Assert.Equal(5, overview[0].TotalLoad);
        Assert.Equal("Medium", overview[0].LoadStatus);
        Assert.False(overview[0].IsHighRisk);

        Assert.Equal(employeeWithLowLoad.Id, overview[1].EmployeeId);
        Assert.Equal("Mette", overview[1].EmployeeName);
        Assert.Equal(0, overview[1].TotalLoad);
        Assert.Equal("Low", overview[1].LoadStatus);
        Assert.False(overview[1].IsHighRisk);
    }
}