using ShiftPlanner.Domain.Shifts;

namespace ShiftPlanner.Application.Scheduling.Analysis;

public class ShiftAssignmentAnalysisResponse
{
    public Guid ShiftId { get; set; }

    public DateOnly Date { get; set; }

    public ShiftType ShiftType { get; set; }

    public string RequiredSkill { get; set; } = string.Empty;

    public bool IsAssigned { get; set; }

    public bool CanBeCovered { get; set; }

    public List<string> SummaryReasons { get; set; } = new();

    public List<ShiftAssignmentCandidateResult> CandidateResults { get; set; } = new();
}