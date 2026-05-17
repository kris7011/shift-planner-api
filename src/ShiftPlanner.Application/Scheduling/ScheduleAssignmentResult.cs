namespace ShiftPlanner.Application.Scheduling;

public class ScheduleAssignmentResult
{
    public Guid ShiftId { get; set; }
    public Guid? EmployeeId { get; set; }
    public string? EmployeeName { get; set; }
    public string RequiredSkill { get; set; } = string.Empty;
    public bool WasAssigned { get; set; }
    public List<string> FailureReasons { get; set; } = [];
}