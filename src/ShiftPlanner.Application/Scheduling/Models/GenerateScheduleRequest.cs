namespace ShiftPlanner.Application.Scheduling.Models;

public class GenerateScheduleRequest
{
    public int MaxAssignmentsPerEmployee { get; set; } = 5;
}