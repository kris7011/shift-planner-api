using ShiftPlanner.Application.Employees;
using ShiftPlanner.Application.Scheduling;
using ShiftPlanner.Application.Scheduling.Overview;
using ShiftPlanner.Application.Scheduling.Rules;
using ShiftPlanner.Application.Scheduling.Simulation;
using ShiftPlanner.Domain.Employees;
using ShiftPlanner.Domain.Shifts;

namespace ShiftPlanner.Tests.Application.Scheduling.Simulation;

public class ScheduleSimulationServiceTests
{
    [Fact]
    public void Simulate_ReturnsHighRisk_WhenShiftCannotBeCovered()
    {
        var employee = new Employee("Kris", new List<string> { "CT" });

        var request = new SimulateScheduleRequest
        {
            Date = new DateOnly(2026, 5, 15),
            ShiftType = ShiftType.Night,
            RequiredSkill = "UL",
            RequiredStaff = 1,
            MaxAssignmentsPerEmployee = 5
        };

        var employees = new List<Employee> { employee };
        var existingShifts = new List<Shift>();

        var loadService = new EmployeeLoadService();
        var statusService = new EmployeeLoadStatusService();

        var rules = new List<ISchedulingRule>
        {
            new MaxAssignmentsRule(),
            new SameDayShiftRule(),
            new NightToDayRule(),
            new HighLoadRule(loadService, statusService)
        };

        var scheduleGeneratorService = new ScheduleGeneratorService(
            loadService,
            rules);

        var service = new ScheduleSimulationService(scheduleGeneratorService);

        var result = service.Simulate(
            request,
            employees,
            existingShifts);

        Assert.False(result.CanBeCovered);
        Assert.Equal("UL", result.RequiredSkill);
        Assert.Equal(ScheduleRiskLevel.High, result.RiskLevel);
        Assert.Null(result.SuggestedEmployeeId);
        Assert.Null(result.SuggestedEmployeeName);

        Assert.Contains(result.FailureReasons, reason =>
            reason.Contains("Missing required skill"));
    }

    [Fact]
    public void Simulate_ReturnsLowRiskAndSuggestedEmployee_WhenShiftCanBeCovered()
    {
        var employee = new Employee("Kris", new List<string> { "CT" });

        var request = new SimulateScheduleRequest
        {
            Date = new DateOnly(2026, 5, 15),
            ShiftType = ShiftType.Day,
            RequiredSkill = "CT",
            RequiredStaff = 1,
            MaxAssignmentsPerEmployee = 5
        };

        var employees = new List<Employee> { employee };
        var existingShifts = new List<Shift>();

        var loadService = new EmployeeLoadService();
        var statusService = new EmployeeLoadStatusService();

        var rules = new List<ISchedulingRule>
    {
        new MaxAssignmentsRule(),
        new SameDayShiftRule(),
        new NightToDayRule(),
        new HighLoadRule(loadService, statusService)
    };

        var scheduleGeneratorService = new ScheduleGeneratorService(
            loadService,
            rules);

        var service = new ScheduleSimulationService(scheduleGeneratorService);

        var result = service.Simulate(
            request,
            employees,
            existingShifts);

        Assert.True(result.CanBeCovered);
        Assert.Equal("CT", result.RequiredSkill);
        Assert.Equal(ScheduleRiskLevel.Low, result.RiskLevel);
        Assert.Equal(employee.Id, result.SuggestedEmployeeId);
        Assert.Equal(employee.Name, result.SuggestedEmployeeName);
        Assert.Empty(result.FailureReasons);
    }
}