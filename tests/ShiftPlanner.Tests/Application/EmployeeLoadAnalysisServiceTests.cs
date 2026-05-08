using ShiftPlanner.Application.Employees;
using ShiftPlanner.Domain.Employees;
using ShiftPlanner.Domain.Shifts;

namespace ShiftPlanner.Tests.Application;

public class EmployeeLoadAnalysisServiceTests
{
    [Fact]
    public void Analyze_ReturnsCorrectAnalysisResult()
    {
        var employee = new Employee("Kris", new List<string> { "CT" });

        var dayShift = new Shift(new DateOnly(2026, 5, 11), ShiftType.Day, "CT", 1);
        var nightShift = new Shift(new DateOnly(2026, 5, 12), ShiftType.Night, "CT", 1);

        dayShift.AssignEmployee(employee);
        nightShift.AssignEmployee(employee);

        var shifts = new List<Shift> { dayShift, nightShift };

        var loadService = new EmployeeLoadService();
        var warningService = new EmployeeLoadWarningService();

        var service = new EmployeeLoadAnalysisService(loadService, warningService);

        var result = service.Analyze(employee, shifts, 4);

        Assert.Equal(employee.Id, result.EmployeeId);
        Assert.Equal("Kris", result.EmployeeName);
        Assert.Equal(5, result.TotalLoad);
        Assert.Equal(4, result.Threshold);
        Assert.True(result.HasHighLoad);
    }
}