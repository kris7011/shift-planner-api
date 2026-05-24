namespace ShiftPlanner.Application.Employees;

public class EmployeeLoadDetailsResponse
{
    public Guid EmployeeId { get; set; }

    public string EmployeeName { get; set; } = string.Empty;

    public List<string> Skills { get; set; } = new();

    public int TotalLoad { get; set; }

    public string LoadStatus { get; set; } = string.Empty;

    public bool IsHighRisk { get; set; }

    public List<EmployeeAssignedShiftLoadItem> AssignedShifts { get; set; } = new();
}