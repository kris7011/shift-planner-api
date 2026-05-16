namespace ShiftPlanner.Application.Scheduling.Models;

public class GenerateScheduleResponse
{
    public string Message { get; set; } = string.Empty;

    public int EmployeeCount { get; set; }

    public int ShiftCount { get; set; }

    public List<ScheduleAssignmentResponse> Assignments { get; set; } = [];
}