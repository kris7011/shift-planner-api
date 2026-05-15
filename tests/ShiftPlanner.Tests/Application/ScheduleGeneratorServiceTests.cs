using ShiftPlanner.Application.Scheduling;
using ShiftPlanner.Domain.Employees;
using ShiftPlanner.Domain.Shifts;

namespace ShiftPlanner.Tests.Application;

public class ScheduleGeneratorServiceTests
{
    [Fact]
    public void Generate_AssignsEmployee_WhenSkillMatches()
    {
        var employee = new Employee("Kris", new List<string> { "CT" });

        var shift = new Shift(
            new DateOnly(2026, 5, 20),
            ShiftType.Day,
            "CT",
            1);

        var employees = new List<Employee> { employee };
        var shifts = new List<Shift> { shift };

        var service = new ScheduleGeneratorService();

        var result = service.Generate(employees, shifts);

        Assert.Single(result);

        Assert.True(result[0].WasAssigned);
        Assert.Equal(employee.Id, result[0].EmployeeId);
    }

    [Fact]
    public void Generate_DoesNotAssignEmployee_WhenSkillDoesNotMatch()
    {
        var employee = new Employee("Kris", new List<string> { "MRI" });

        var shift = new Shift(
            new DateOnly(2026, 5, 20),
            ShiftType.Day,
            "CT",
            1);

        var employees = new List<Employee> { employee };
        var shifts = new List<Shift> { shift };

        var service = new ScheduleGeneratorService();

        var result = service.Generate(employees, shifts);

        Assert.Single(result);

        Assert.False(result[0].WasAssigned);
        Assert.Null(result[0].EmployeeId);
    }
}