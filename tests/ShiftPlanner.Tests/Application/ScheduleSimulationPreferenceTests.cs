using ShiftPlanner.Application.Scheduling.Simulation;
using ShiftPlanner.Domain.Employees;
using ShiftPlanner.Domain.Shifts;
using ShiftPlanner.Tests.Helpers;

namespace ShiftPlanner.Tests.Application;

public class ScheduleSimulationPreferenceTests
{
    [Fact]
    public void Simulate_AddsPreferenceBonus_WhenEmployeePrefersShiftType()
    {
        var employees = new List<Employee>
        {
            new("Henrik", new List<string> { "Night" }),
            new("Anna", new List<string> { "Night" })
        };

        var existingShifts = new List<Shift>();

        var request = new SimulateScheduleRequest
        {
            Date = new DateOnly(2026, 5, 12),
            ShiftType = ShiftType.Night,
            RequiredSkill = "Night",
            RequiredStaff = 1,
            MaxAssignmentsPerEmployee = 5
        };

        var service = ScheduleSimulationServiceFactory.Create();

        var result = service.Simulate(
            request,
            employees,
            existingShifts);

        var henrik = result.CandidateResults.Single(candidate =>
            candidate.EmployeeName == "Henrik");

        var anna = result.CandidateResults.Single(candidate =>
            candidate.EmployeeName == "Anna");

        Assert.True(henrik.Score > anna.Score);
        Assert.Contains("Employee prefers Night shifts.", henrik.Reasons);
    }
}