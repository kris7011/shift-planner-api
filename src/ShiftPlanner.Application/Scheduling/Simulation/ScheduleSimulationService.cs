using ShiftPlanner.Application.Employees.Preferences;
using ShiftPlanner.Application.Scheduling.Overview;
using ShiftPlanner.Domain.Employees;
using ShiftPlanner.Domain.Shifts;

namespace ShiftPlanner.Application.Scheduling.Simulation;

public class ScheduleSimulationService : IScheduleSimulationService
{
    private readonly IScheduleGeneratorService _scheduleGeneratorService;
    private readonly EmployeePreferenceProfileProvider _preferenceProfileProvider;
    private readonly EmployeePreferenceScoreService _preferenceScoreService;

    public ScheduleSimulationService(
        IScheduleGeneratorService scheduleGeneratorService,
        EmployeePreferenceProfileProvider preferenceProfileProvider,
        EmployeePreferenceScoreService preferenceScoreService)
    {
        _scheduleGeneratorService = scheduleGeneratorService;
        _preferenceProfileProvider = preferenceProfileProvider;
        _preferenceScoreService = preferenceScoreService;
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

        var impactIndicators = CreateImpactIndicators(
            canBeCovered,
            request.RequiredSkill,
            assignmentResult.FailureReasons);

        var candidateResults = CreateCandidateResults(
            employees,
            assignmentResult,
            simulatedShift,
            existingShifts);

        return new SimulateScheduleResponse
        {
            CanBeCovered = canBeCovered,
            RequiredSkill = request.RequiredSkill,
            RiskLevel = riskLevel,
            SuggestedEmployeeId = assignmentResult.EmployeeId,
            SuggestedEmployeeName = assignmentResult.EmployeeName,
            FailureReasons = assignmentResult.FailureReasons,
            ImpactSummary = impactSummary,
            ImpactIndicators = impactIndicators,
            CandidateResults = candidateResults
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

    private static List<SimulationImpactIndicator> CreateImpactIndicators(
        bool canBeCovered,
        string requiredSkill,
        List<string> failureReasons)
    {
        if (canBeCovered)
        {
            return
            [
                new SimulationImpactIndicator
                {
                    Type = "Coverage",
                    Severity = ScheduleRiskLevel.Low,
                    Message = "The simulated shift can be covered."
                }
            ];
        }

        var indicators = new List<SimulationImpactIndicator>
        {
            new()
            {
                Type = "Coverage",
                Severity = ScheduleRiskLevel.High,
                Message = "The simulated shift cannot be covered."
            }
        };

        if (failureReasons.Any(reason =>
            reason.Contains("Missing required skill", StringComparison.OrdinalIgnoreCase)))
        {
            indicators.Add(new SimulationImpactIndicator
            {
                Type = "Skill",
                Severity = ScheduleRiskLevel.High,
                Message = $"No available employee can satisfy the required skill '{requiredSkill}'."
            });
        }

        if (failureReasons.Any(reason =>
            reason.Contains("night shift", StringComparison.OrdinalIgnoreCase) ||
            reason.Contains("day shift", StringComparison.OrdinalIgnoreCase)))
        {
            indicators.Add(new SimulationImpactIndicator
            {
                Type = "RestRule",
                Severity = ScheduleRiskLevel.High,
                Message = "The simulated shift conflicts with rest-time or shift sequence rules."
            });
        }

        return indicators;
    }

    private List<SimulationCandidateResult> CreateCandidateResults(
        List<Employee> employees,
        ScheduleAssignmentResult assignmentResult,
        Shift simulatedShift,
        List<Shift> existingShifts)
    {
        return employees
            .Select(employee =>
            {
                var employeeReasons = assignmentResult.FailureReasons
                    .Where(reason => reason.StartsWith($"{employee.Name}:"))
                    .Select(reason => reason.Replace($"{employee.Name}: ", string.Empty))
                    .ToList();

                var canBeAssigned = assignmentResult.EmployeeId == employee.Id;

                var employeeShifts = existingShifts
                    .Where(existingShift => existingShift.EmployeeId == employee.Id)
                    .ToList();

                var preferenceProfile = _preferenceProfileProvider.GetProfile(employee);

                var preferenceScore = _preferenceScoreService.CalculateScoreAdjustment(
                    preferenceProfile,
                    simulatedShift,
                    employeeShifts);

                var baseScore = canBeAssigned ? 100 : 0;
                var finalScore = baseScore + preferenceScore.ScoreAdjustment;

                var reasons = canBeAssigned
                    ? new List<string>()
                    : employeeReasons;

                reasons.AddRange(preferenceScore.Reasons);

                return new SimulationCandidateResult
                {
                    EmployeeId = employee.Id,
                    EmployeeName = employee.Name,
                    CanBeAssigned = canBeAssigned,
                    Score = finalScore,
                    Reasons = reasons
                };
            })
            .OrderByDescending(candidate => candidate.Score)
            .ToList();
    }
}