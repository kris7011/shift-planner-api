using ShiftPlanner.Application.Scheduling.Overview;
using ShiftPlanner.Domain.Employees;
using ShiftPlanner.Domain.Shifts;

namespace ShiftPlanner.Application.Scheduling.Simulation;

public class ScheduleSimulationService : IScheduleSimulationService
{
    private readonly IScheduleGeneratorService _scheduleGeneratorService;

    public ScheduleSimulationService(IScheduleGeneratorService scheduleGeneratorService)
    {
        _scheduleGeneratorService = scheduleGeneratorService;
    }

    public SimulateScheduleResponse Simulate(
        SimulateScheduleRequest request,
        List<Employee> employees,
        List<Shift> existingShifts)
    {
        var simulatedShift = new Shift(
            request.Date,
            request.ShiftType,
            request.RequiredSkill,
            request.RequiredStaff);

        var results = _scheduleGeneratorService.Generate(
            employees,
            openShifts: new List<Shift> { simulatedShift },
            existingShifts: existingShifts,
            maxAssignmentsPerEmployee: request.MaxAssignmentsPerEmployee);

        var assignmentResult = results.First();

        var canBeCovered = assignmentResult.WasAssigned;

        var riskLevel = canBeCovered
            ? ScheduleRiskLevel.Low
            : ScheduleRiskLevel.High;

        var impactSummary = CreateImpactSummary(
            canBeCovered,
            request.RequiredSkill,
            assignmentResult.EmployeeName);

        return new SimulateScheduleResponse
        {
            CanBeCovered = canBeCovered,
            RequiredSkill = request.RequiredSkill,
            RiskLevel = riskLevel,
            SuggestedEmployeeId = assignmentResult.EmployeeId,
            SuggestedEmployeeName = assignmentResult.EmployeeName,
            FailureReasons = assignmentResult.FailureReasons,
            ImpactSummary = impactSummary
        };
    }

    private static string CreateImpactSummary(
        bool canBeCovered,
        string requiredSkill,
        string? suggestedEmployeeName)
    {
        if (canBeCovered)
        {
            return $"This shift can be covered by {suggestedEmployeeName} with low scheduling risk.";
        }

        return $"This shift cannot be covered because no available employee can satisfy the required skill '{requiredSkill}' and scheduling rules.";
    }
}