namespace ShiftPlanner.Application.Employees.Preferences;

public class EmployeePreferenceScoreResult
{
    public int ScoreAdjustment { get; set; }

    public List<string> Reasons { get; set; } = new();
}