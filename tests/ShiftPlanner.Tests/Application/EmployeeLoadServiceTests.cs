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
}