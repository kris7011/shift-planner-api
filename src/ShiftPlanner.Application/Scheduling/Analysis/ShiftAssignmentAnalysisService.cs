using ShiftPlanner.Domain.Employees;
using ShiftPlanner.Domain.Shifts;

namespace ShiftPlanner.Application.Scheduling.Analysis;

public class ShiftAssignmentAnalysisService
{
    public ShiftAssignmentAnalysisResponse Analyze(
        Shift shift,
        List<Employee> employees,
        List<Shift> plannedShifts,
        int maxAssignmentsPerEmployee)
    {
        var candidateResults = employees
            .Select(employee => AnalyzeCandidate(
                employee,
                shift,
                plannedShifts,
                maxAssignmentsPerEmployee))
            .ToList();

        var canBeCovered = candidateResults.Any(candidate => candidate.CanBeAssigned);

        return new ShiftAssignmentAnalysisResponse
        {
            ShiftId = shift.Id,
            Date = shift.Date,
            ShiftType = shift.ShiftType,
            RequiredSkill = shift.RequiredSkill,
            IsAssigned = shift.EmployeeId != null,
            CanBeCovered = canBeCovered,
            SummaryReasons = CreateSummaryReasons(shift, employees, canBeCovered),
            CandidateResults = candidateResults
        };
    }

    private static ShiftAssignmentCandidateResult AnalyzeCandidate(
        Employee employee,
        Shift shift,
        List<Shift> plannedShifts,
        int maxAssignmentsPerEmployee)
    {
        var reasons = new List<string>();

        if (!employee.Skills.Contains(shift.RequiredSkill))
        {
            reasons.Add($"Missing required skill '{shift.RequiredSkill}'.");
        }

        var employeeShifts = plannedShifts
            .Where(plannedShift => IsAssignedToEmployee(plannedShift, employee))
            .ToList();

        if (employeeShifts.Count >= maxAssignmentsPerEmployee)
        {
            reasons.Add($"Employee has reached the maximum of {maxAssignmentsPerEmployee} assignments.");
        }

        var alreadyAssignedSameDay = employeeShifts.Any(plannedShift =>
            plannedShift.Date == shift.Date);

        if (alreadyAssignedSameDay)
        {
            reasons.Add("Employee is already assigned to a shift on the same day.");
        }

        var hasNightShiftPreviousDay = employeeShifts.Any(plannedShift =>
            plannedShift.Date == shift.Date.AddDays(-1) &&
            plannedShift.ShiftType == ShiftType.Night);

        if (hasNightShiftPreviousDay && shift.ShiftType == ShiftType.Day)
        {
            reasons.Add("Employee cannot work a day shift immediately after a night shift.");
        }

        return new ShiftAssignmentCandidateResult
        {
            EmployeeId = employee.Id,
            EmployeeName = employee.Name,
            CanBeAssigned = reasons.Count == 0,
            Reasons = reasons
        };
    }

    private static List<string> CreateSummaryReasons(
        Shift shift,
        List<Employee> employees,
        bool canBeCovered)
    {
        var reasons = new List<string>();

        if (canBeCovered)
        {
            reasons.Add("At least one employee can cover this shift.");
            return reasons;
        }

        var employeesWithRequiredSkill = employees
            .Where(employee => employee.Skills.Contains(shift.RequiredSkill))
            .ToList();

        if (employeesWithRequiredSkill.Count == 0)
        {
            reasons.Add($"No employees have the required skill '{shift.RequiredSkill}'.");
        }
        else
        {
            reasons.Add("Employees with the required skill are blocked by scheduling rules.");
        }

        return reasons;
    }

    private static bool IsAssignedToEmployee(Shift shift, Employee employee)
    {
        if (shift.EmployeeId == employee.Id)
        {
            return true;
        }

        return shift.AssignedEmployees.Any(assignedEmployee =>
            assignedEmployee.Id == employee.Id);
    }
}