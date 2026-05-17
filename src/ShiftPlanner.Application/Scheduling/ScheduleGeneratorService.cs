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
            var candidates = new List<Employee>();
            var failureReasons = new List<string>();

            foreach (var employee in employees)
            {
                if (!employee.HasSkill(shift.RequiredSkill))
                {
                    failureReasons.Add(
                        $"{employee.Name}: Missing required skill '{shift.RequiredSkill}'.");

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
                    failureReasons.AddRange(
                        failedRules
                            .Where(result => !string.IsNullOrWhiteSpace(result.FailureReason))
                            .Select(result => $"{employee.Name}: {result.FailureReason}"));

                    continue;
                }

                candidates.Add(employee);
            }

            var matchingEmployee = candidates
                .OrderBy(employee =>
                {
                    var employeeShifts = plannedShifts
                        .Where(existingShift => existingShift.EmployeeId == employee.Id)
                        .ToList();

                    return _employeeLoadService.CalculateLoad(employeeShifts);
                })
                .FirstOrDefault();

            if (matchingEmployee != null)
            {
                shift.AssignToEmployee(matchingEmployee.Id);
                plannedShifts.Add(shift);
            }

            results.Add(new ScheduleAssignmentResult
            {
                ShiftId = shift.Id,
                EmployeeId = matchingEmployee?.Id,
                EmployeeName = matchingEmployee?.Name,
                RequiredSkill = shift.RequiredSkill,
                WasAssigned = matchingEmployee != null,
                FailureReasons = matchingEmployee == null
                    ? failureReasons
                    : []
            });
        }

        return results;
    }
}