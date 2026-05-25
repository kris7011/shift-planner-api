using ShiftPlanner.Application.Scheduling.Analysis;
using ShiftPlanner.Domain.Employees;
using ShiftPlanner.Domain.Shifts;

namespace ShiftPlanner.Tests.Application;

public class ShiftAssignmentAnalysisServiceTests
{
    [Fact]
    public void Analyze_ReturnsSkillGapReason_WhenNoEmployeeHasRequiredSkill()
    {
        var employee = new Employee("Kris", new List<string> { "CT" });

        var shift = new Shift(
            new DateOnly(2026, 5, 15),
            ShiftType.Day,
            "UL",
            1);

        var employees = new List<Employee>
        {
            employee
        };

        var plannedShifts = new List<Shift>();

        var service = new ShiftAssignmentAnalysisService();

        var result = service.Analyze(
            shift,
            employees,
            plannedShifts,
            maxAssignmentsPerEmployee: 5);

        Assert.Equal(shift.Id, result.ShiftId);
        Assert.Equal("UL", result.RequiredSkill);
        Assert.False(result.IsAssigned);
        Assert.False(result.CanBeCovered);

        Assert.Single(result.SummaryReasons);
        Assert.Equal("No employees have the required skill 'UL'.", result.SummaryReasons[0]);

        Assert.Single(result.CandidateResults);
        Assert.Equal(employee.Id, result.CandidateResults[0].EmployeeId);
        Assert.Equal("Kris", result.CandidateResults[0].EmployeeName);
        Assert.False(result.CandidateResults[0].CanBeAssigned);
        Assert.Contains(
            "Missing required skill 'UL'.",
            result.CandidateResults[0].Reasons);
    }

    [Fact]
    public void Analyze_ReturnsCanBeCovered_WhenEmployeeHasRequiredSkillAndNoRuleBlocks()
    {
        var employee = new Employee("Mette", new List<string> { "CT" });

        var shift = new Shift(
            new DateOnly(2026, 5, 15),
            ShiftType.Day,
            "CT",
            1);

        var employees = new List<Employee>
        {
            employee
        };

        var plannedShifts = new List<Shift>();

        var service = new ShiftAssignmentAnalysisService();

        var result = service.Analyze(
            shift,
            employees,
            plannedShifts,
            maxAssignmentsPerEmployee: 5);

        Assert.True(result.CanBeCovered);
        Assert.Single(result.SummaryReasons);
        Assert.Equal("At least one employee can cover this shift.", result.SummaryReasons[0]);

        Assert.Single(result.CandidateResults);
        Assert.Equal(employee.Id, result.CandidateResults[0].EmployeeId);
        Assert.Equal("Mette", result.CandidateResults[0].EmployeeName);
        Assert.True(result.CandidateResults[0].CanBeAssigned);
        Assert.Empty(result.CandidateResults[0].Reasons);
    }
}