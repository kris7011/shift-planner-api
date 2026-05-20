namespace ShiftPlanner.Application.Scheduling.Simulation;

public class SimulationCandidateResult
{
    public Guid EmployeeId { get; set; }

    public string EmployeeName { get; set; } = string.Empty;

    public bool CanBeAssigned { get; set; }

    public int Score { get; set; }

    public List<string> Reasons { get; set; } = [];
}