namespace ShiftPlanner.Application.Employees;

public class EmployeeLoadAnalysisResult
{
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public int TotalLoad { get; set; }
    public int Threshold { get; set; }
    public bool HasHighLoad { get; set; }
}