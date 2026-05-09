using ShiftPlanner.Application.Employees;
using ShiftPlanner.Domain.Employees;
using ShiftPlanner.Domain.Shifts;

namespace ShiftPlanner.Tests.Application;

public class EmployeeLoadServiceTests
{
    [Fact]
    public void CalculateTotalLoad_ReturnsSumOfAssignedShiftsOnly()
    {
        var employee = new Employee("Kris", new List<string> { "CT" });
        var otherEmployee = new Employee("Mette", new List<string> { "CT" });

        var dayShift = new Shift(new DateOnly(2026, 5, 11), ShiftType.Day, "CT", 1);
        var nightShift = new Shift(new DateOnly(2026, 5, 12), ShiftType.Night, "CT", 1);
        var eveningShift = new Shift(new DateOnly(2026, 5, 13), ShiftType.Evening, "CT", 1);

        dayShift.AssignEmployee(employee);
        nightShift.AssignEmployee(employee);
        eveningShift.AssignEmployee(otherEmployee);

        var shifts = new List<Shift>
        {
            dayShift,
            nightShift,
            eveningShift
        };

        var service = new EmployeeLoadService();

        var totalLoad = service.CalculateTotalLoad(employee, shifts);

        Assert.Equal(5, totalLoad);
    }

    [Fact]
    public void CalculateLoad_ReturnsZero_WhenNoShifts()
    {
        var service = new EmployeeLoadService();

        var result = service.CalculateLoad(new List<Shift>());

        Assert.Equal(0, result);
    }

    [Fact]
    public void CalculateLoad_ReturnsCorrectScore_ForOneDayShift()
    {
        var service = new EmployeeLoadService();

        var shifts = new List<Shift>
    {
        new Shift(new DateOnly(2026, 5, 11), ShiftType.Day, "CT", 1)
    };

        var result = service.CalculateLoad(shifts);

        Assert.Equal(1, result);
    }

    [Fact]
    public void CalculateLoad_ReturnsCorrectScore_ForOneNightShift()
    {
        var service = new EmployeeLoadService();

        var shifts = new List<Shift>
    {
        new Shift(new DateOnly(2026, 5, 11), ShiftType.Night, "CT", 1)
    };

        var result = service.CalculateLoad(shifts);

        Assert.Equal(4, result);
    }

    [Fact]
    public void CalculateLoad_ReturnsCorrectScore_ForMixedShifts()
    {
        var service = new EmployeeLoadService();

        var shifts = new List<Shift>
    {
        new Shift(new DateOnly(2026, 5, 11), ShiftType.Day, "CT", 1),
        new Shift(new DateOnly(2026, 5, 12), ShiftType.Evening, "CT", 1),
        new Shift(new DateOnly(2026, 5, 13), ShiftType.Night, "CT", 1)
    };

        var result = service.CalculateLoad(shifts);

        Assert.Equal(7, result);
    }
}