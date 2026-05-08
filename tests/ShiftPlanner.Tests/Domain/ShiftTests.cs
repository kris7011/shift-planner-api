using ShiftPlanner.Domain.Employees;
using ShiftPlanner.Domain.Shifts;

namespace ShiftPlanner.Tests.Domain;

public class ShiftTests
{
    [Fact]
    public void IsFullyStaffed_ReturnsFalse_WhenRequiredStaffIsNotMet()
    {
        var shift = new Shift(
            new DateOnly(2026, 5, 11),
            ShiftType.Day,
            "CT",
            2);

        var employee = new Employee("Kris", new List<string> { "CT" });

        shift.AssignEmployee(employee);

        Assert.False(shift.IsFullyStaffed());
    }

    [Fact]
    public void IsFullyStaffed_ReturnsTrue_WhenRequiredStaffIsMet()
    {
        var shift = new Shift(
            new DateOnly(2026, 5, 11),
            ShiftType.Day,
            "CT",
            1);

        var employee = new Employee("Kris", new List<string> { "CT" });

        shift.AssignEmployee(employee);

        Assert.True(shift.IsFullyStaffed());
    }

    [Fact]
    public void MissingStaffCount_ReturnsCorrectNumber()
    {
        var shift = new Shift(
            new DateOnly(2026, 5, 11),
            ShiftType.Evening,
            "MRI",
            3);

        var employee = new Employee("Mette", new List<string> { "MRI" });

        shift.AssignEmployee(employee);

        Assert.Equal(2, shift.MissingStaffCount());
    }

    [Fact]
    public void AssignEmployee_ThrowsException_WhenEmployeeDoesNotHaveRequiredSkill()
    {
        var shift = new Shift(
            new DateOnly(2026, 5, 11),
            ShiftType.Night,
            "CT",
            1);

        var employee = new Employee("Henrik", new List<string> { "XRay" });

        Assert.Throws<InvalidOperationException>(() => shift.AssignEmployee(employee));
    }
}