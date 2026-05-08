using ShiftPlanner.Domain.Shifts;

namespace ShiftPlanner.Api.LoadAnalysis;

public class LoadAnalysisRequest
{
    public string EmployeeName { get; set; } = string.Empty;
    public List<string> Skills { get; set; } = new();
    public int Threshold { get; set; }
    public List<LoadAnalysisShiftRequest> Shifts { get; set; } = new();
}

public class LoadAnalysisShiftRequest
{
    public DateOnly Date { get; set; }
    public ShiftType ShiftType { get; set; }
    public string RequiredSkill { get; set; } = string.Empty;
    public int RequiredStaff { get; set; }
    public bool AssignEmployee { get; set; }
}