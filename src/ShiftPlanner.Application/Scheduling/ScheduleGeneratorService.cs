using ShiftPlanner.Application.Employees;
using ShiftPlanner.Application.Scheduling.Rules;
using ShiftPlanner.Domain.Employees;
using ShiftPlanner.Domain.Shifts;

namespace ShiftPlanner.Application.Scheduling;

public class ScheduleGeneratorService : IScheduleGeneratorService
{
    private readonly IEmployeeLoadService _employeeLoadService;

    private readonly IEnumerable<ISchedulingRule> _rules;

    public ScheduleGeneratorService(
        IEmployeeLoadService employeeLoadService,
        IEnumerable<ISchedulingRule> rules)
    {
        _employeeLoadService = employeeLoadService;
        _rules = rules;
    }

    public List<ScheduleAssignmentResult> Generate(
    List<Employee> employees,
    List<Shift> openShifts,
    List<Shift> existingShifts,
    int maxAssignmentsPerEmployee)
    {
        var results = new List<ScheduleAssignmentResult>();
        var plannedShifts = new List<Shift>(existingShifts);

        foreach (var shift in openShifts)
        {
            var evaluations = EvaluateCandidates(
                employees,
                shift,
                plannedShifts,
                maxAssignmentsPerEmployee);

            foreach (var employee in employees)
            {
                var evaluation = new CandidateEvaluation
                {
                    Employee = employee,
                    CanAssign = true
                };

                if (!employee.HasSkill(shift.RequiredSkill))
                {
                    evaluation.CanAssign = false;
                    evaluation.FailureReasons.Add(
                        $"{employee.Name}: Missing required skill '{shift.RequiredSkill}'.");

                    evaluations.Add(evaluation);

                    continue;
                }

                var context = new SchedulingRuleContext
                {
                    Employee = employee,
                    Shift = shift,
                    PlannedShifts = plannedShifts,
                    MaxAssignmentsPerEmployee = maxAssignmentsPerEmployee
                };

                var ruleResults = _rules
                    .Select(rule => rule.Evaluate(context))
                    .ToList();

                var failedRules = ruleResults
                    .Where(result => !result.Success)
                    .ToList();

                if (failedRules.Count > 0)
                {
                    evaluation.CanAssign = false;

                    evaluation.FailureReasons.AddRange(
                        failedRules
                            .Where(result => !string.IsNullOrWhiteSpace(result.FailureReason))
                            .Select(result => $"{employee.Name}: {result.FailureReason}"));
                }

                evaluations.Add(evaluation);
            }

            var matchingEmployee = SelectBestCandidate(
                evaluations,
                plannedShifts);

            if (matchingEmployee != null)
            {
                shift.AssignToEmployee(matchingEmployee.Id);
                plannedShifts.Add(shift);
            }

            results.Add(CreateAssignmentResult(
                shift,
                matchingEmployee,
                evaluations));
        }

        return results;
    }

    private List<CandidateEvaluation> EvaluateCandidates(
    List<Employee> employees,
    Shift shift,
    List<Shift> plannedShifts,
    int maxAssignmentsPerEmployee)
    {
        var evaluations = new List<CandidateEvaluation>();

        foreach (var employee in employees)
        {
            var evaluation = new CandidateEvaluation
            {
                Employee = employee,
                CanAssign = true
            };

            if (!employee.HasSkill(shift.RequiredSkill))
            {
                evaluation.CanAssign = false;
                evaluation.FailureReasons.Add(
                    $"{employee.Name}: Missing required skill '{shift.RequiredSkill}'.");

                evaluations.Add(evaluation);

                continue;
            }

            var context = new SchedulingRuleContext
            {
                Employee = employee,
                Shift = shift,
                PlannedShifts = plannedShifts,
                MaxAssignmentsPerEmployee = maxAssignmentsPerEmployee
            };

            var ruleResults = _rules
                .Select(rule => rule.Evaluate(context))
                .ToList();

            var failedRules = ruleResults
                .Where(result => !result.Success)
                .ToList();

            if (failedRules.Count > 0)
            {
                evaluation.CanAssign = false;

                evaluation.FailureReasons.AddRange(
                    failedRules
                        .Where(result => !string.IsNullOrWhiteSpace(result.FailureReason))
                        .Select(result => $"{employee.Name}: {result.FailureReason}"));
            }

            evaluations.Add(evaluation);
        }

        return evaluations;
    }

    private Employee? SelectBestCandidate(
    List<CandidateEvaluation> evaluations,
    List<Shift> plannedShifts)
    {
        return evaluations
            .Where(evaluation => evaluation.CanAssign)
            .Select(evaluation => evaluation.Employee)
            .OrderBy(employee =>
            {
                var employeeShifts = plannedShifts
                    .Where(existingShift => existingShift.EmployeeId == employee.Id)
                    .ToList();

                return _employeeLoadService.CalculateLoad(employeeShifts);
            })
            .FirstOrDefault();
    }

    private static ScheduleAssignmentResult CreateAssignmentResult(
    Shift shift,
    Employee? matchingEmployee,
    List<CandidateEvaluation> evaluations)
    {
        var failureReasons = evaluations
            .Where(evaluation => !evaluation.CanAssign)
            .SelectMany(evaluation => evaluation.FailureReasons)
            .Distinct()
            .ToList();

        return new ScheduleAssignmentResult
        {
            ShiftId = shift.Id,
            EmployeeId = matchingEmployee?.Id,
            EmployeeName = matchingEmployee?.Name,
            RequiredSkill = shift.RequiredSkill,
            WasAssigned = matchingEmployee != null,
            FailureReasons = matchingEmployee == null
                ? failureReasons
                : []
        };
    }
}