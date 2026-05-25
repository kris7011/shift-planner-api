namespace ShiftPlanner.Application.Scheduling.Analysis;

public class ShiftAssignmentCandidateResult
{
    public Guid EmployeeId { get; set; }

    public string EmployeeName { get; set; } = string.Empty;

    public bool CanBeAssigned { get; set; }

    public List<string> Reasons { get; set; } = new();
}