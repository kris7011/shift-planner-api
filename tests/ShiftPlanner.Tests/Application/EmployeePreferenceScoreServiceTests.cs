using ShiftPlanner.Application.Employees.Preferences;
using ShiftPlanner.Domain.Employees.Preferences;
using ShiftPlanner.Domain.Shifts;

namespace ShiftPlanner.Tests.Application;

public class EmployeePreferenceScoreServiceTests
{
    [Fact]
    public void CalculateScoreAdjustment_AddsBonus_WhenShiftTypeIsPreferred()
    {
        var profile = new EmployeePreferenceProfile
        {
            PreferredShiftTypes = new List<ShiftType>
            {
                ShiftType.Night
            }
        };

        var shift = new Shift(
            new DateOnly(2026, 5, 12),
            ShiftType.Night,
            "Night",
            1);

        var service = new EmployeePreferenceScoreService();

        var result = service.CalculateScoreAdjustment(
            profile,
            shift,
            new List<Shift>());

        Assert.Equal(15, result.ScoreAdjustment);
        Assert.Contains("Employee prefers Night shifts.", result.Reasons);
    }

    [Fact]
    public void CalculateScoreAdjustment_AddsPenalty_WhenShiftTypeIsDisliked()
    {
        var profile = new EmployeePreferenceProfile
        {
            DislikedShiftTypes = new List<ShiftType>
            {
                ShiftType.Night
            }
        };

        var shift = new Shift(
            new DateOnly(2026, 5, 12),
            ShiftType.Night,
            "Night",
            1);

        var service = new EmployeePreferenceScoreService();

        var result = service.CalculateScoreAdjustment(
            profile,
            shift,
            new List<Shift>());

        Assert.Equal(-20, result.ScoreAdjustment);
        Assert.Contains("Employee dislikes Night shifts.", result.Reasons);
    }

    [Fact]
    public void CalculateScoreAdjustment_AddsPenalty_WhenMaxNightShiftsReached()
    {
        var profile = new EmployeePreferenceProfile
        {
            MaxNightShifts = 1
        };

        var shift = new Shift(
            new DateOnly(2026, 5, 13),
            ShiftType.Night,
            "Night",
            1);

        var existingNightShift = new Shift(
            new DateOnly(2026, 5, 12),
            ShiftType.Night,
            "Night",
            1);

        var service = new EmployeePreferenceScoreService();

        var result = service.CalculateScoreAdjustment(
            profile,
            shift,
            new List<Shift>
            {
                existingNightShift
            });

        Assert.Equal(-30, result.ScoreAdjustment);
        Assert.Contains(
            "Employee has reached preferred maximum night shifts (1).",
            result.Reasons);
    }

    [Fact]
    public void CalculateScoreAdjustment_AddsWeekendPenalty_WhenEmployeeAvoidsWeekends()
    {
        var profile = new EmployeePreferenceProfile
        {
            AvoidsWeekends = true
        };

        var shift = new Shift(
            new DateOnly(2026, 5, 16),
            ShiftType.Day,
            "CT",
            1);

        var service = new EmployeePreferenceScoreService();

        var result = service.CalculateScoreAdjustment(
            profile,
            shift,
            new List<Shift>());

        Assert.Equal(-15, result.ScoreAdjustment);
        Assert.Contains(
            "Employee prefers to avoid weekend shifts.",
            result.Reasons);
    }
}