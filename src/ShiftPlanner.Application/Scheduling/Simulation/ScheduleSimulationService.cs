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

        return new SimulateScheduleResponse
        {
            CanBeCovered = assignmentResult.WasAssigned,
            RequiredSkill = request.RequiredSkill,
            RiskLevel = assignmentResult.WasAssigned
                ? ScheduleRiskLevel.Low
                : ScheduleRiskLevel.High,
            SuggestedEmployeeId = assignmentResult.EmployeeId,
            SuggestedEmployeeName = assignmentResult.EmployeeName,
            FailureReasons = assignmentResult.FailureReasons
        };
    }
}