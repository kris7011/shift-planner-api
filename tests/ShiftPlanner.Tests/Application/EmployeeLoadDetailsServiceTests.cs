using ShiftPlanner.Application.Employees;
using ShiftPlanner.Domain.Employees;
using ShiftPlanner.Domain.Shifts;

namespace ShiftPlanner.Tests.Application;

public class EmployeeLoadDetailsServiceTests
{
    [Fact]
    public void CreateDetails_ReturnsAssignedShiftsWithLoadScores()
    {
        var employee = new Employee("Henrik", new List<string> { "CT", "Night" });
        var otherEmployee = new Employee("Mette", new List<string> { "MRI" });

        var dayShift = new Shift(
            new DateOnly(2026, 5, 11),
            ShiftType.Day,
            "CT",
            1);

        var nightShift = new Shift(
            new DateOnly(2026, 5, 12),
            ShiftType.Night,
            "Night",
            1);

        var eveningShift = new Shift(
            new DateOnly(2026, 5, 13),
            ShiftType.Evening,
            "MRI",
            1);

        dayShift.AssignEmployee(employee);
        nightShift.AssignEmployee(employee);
        eveningShift.AssignEmployee(otherEmployee);

        var shifts = new List<Shift>
        {
            eveningShift,
            nightShift,
            dayShift
        };

        var loadService = new EmployeeLoadService();
        var statusService = new EmployeeLoadStatusService();
        var detailsService = new EmployeeLoadDetailsService(loadService, statusService);

        var details = detailsService.CreateDetails(employee, shifts);

        Assert.Equal(employee.Id, details.EmployeeId);
        Assert.Equal("Henrik", details.EmployeeName);
        Assert.Equal(5, details.TotalLoad);
        Assert.Equal("Medium", details.LoadStatus);
        Assert.False(details.IsHighRisk);

        Assert.Equal(2, details.AssignedShifts.Count);

        Assert.Equal(dayShift.Id, details.AssignedShifts[0].ShiftId);
        Assert.Equal(new DateOnly(2026, 5, 11), details.AssignedShifts[0].Date);
        Assert.Equal(ShiftType.Day, details.AssignedShifts[0].ShiftType);
        Assert.Equal("CT", details.AssignedShifts[0].RequiredSkill);
        Assert.Equal(1, details.AssignedShifts[0].LoadScore);

        Assert.Equal(nightShift.Id, details.AssignedShifts[1].ShiftId);
        Assert.Equal(new DateOnly(2026, 5, 12), details.AssignedShifts[1].Date);
        Assert.Equal(ShiftType.Night, details.AssignedShifts[1].ShiftType);
        Assert.Equal("Night", details.AssignedShifts[1].RequiredSkill);
        Assert.Equal(4, details.AssignedShifts[1].LoadScore);
    }
}