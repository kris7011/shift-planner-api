using ShiftPlanner.Domain.Employees;

namespace ShiftPlanner.Application.Scheduling;

public class CandidateEvaluation
{
    public Employee Employee { get; set; } = null!;

    public bool CanAssign { get; set; }

    public List<string> FailureReasons { get; set; } = [];
}