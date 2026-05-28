using ShiftPlanner.Domain.Employees.Preferences;
using ShiftPlanner.Domain.Shifts;

namespace ShiftPlanner.Application.Employees.Preferences;

public class EmployeePreferenceScoreService
{
    public EmployeePreferenceScoreResult CalculateScoreAdjustment(
        EmployeePreferenceProfile profile,
        Shift shift,
        List<Shift> employeeShifts)
    {
        var result = new EmployeePreferenceScoreResult();

        if (profile.PreferredShiftTypes.Contains(shift.ShiftType))
        {
            result.ScoreAdjustment += 15;
            result.Reasons.Add($"Employee prefers {shift.ShiftType} shifts.");
        }

        if (profile.DislikedShiftTypes.Contains(shift.ShiftType))
        {
            result.ScoreAdjustment -= 20;
            result.Reasons.Add($"Employee dislikes {shift.ShiftType} shifts.");
        }

        if (shift.ShiftType == ShiftType.Night &&
            profile.MaxNightShifts.HasValue)
        {
            var currentNightShifts = employeeShifts.Count(existingShift =>
                existingShift.ShiftType == ShiftType.Night);

            if (currentNightShifts >= profile.MaxNightShifts.Value)
            {
                result.ScoreAdjustment -= 30;
                result.Reasons.Add(
                    $"Employee has reached preferred maximum night shifts ({profile.MaxNightShifts.Value}).");
            }
        }

        if (shift.ShiftType == ShiftType.Evening &&
            profile.MaxEveningShifts.HasValue)
        {
            var currentEveningShifts = employeeShifts.Count(existingShift =>
                existingShift.ShiftType == ShiftType.Evening);

            if (currentEveningShifts >= profile.MaxEveningShifts.Value)
            {
                result.ScoreAdjustment -= 20;
                result.Reasons.Add(
                    $"Employee has reached preferred maximum evening shifts ({profile.MaxEveningShifts.Value}).");
            }
        }

        if (IsWeekend(shift.Date) && profile.PrefersWeekends)
        {
            result.ScoreAdjustment += 10;
            result.Reasons.Add("Employee prefers weekend shifts.");
        }

        if (IsWeekend(shift.Date) && profile.AvoidsWeekends)
        {
            result.ScoreAdjustment -= 15;
            result.Reasons.Add("Employee prefers to avoid weekend shifts.");
        }

        return result;
    }

    private static bool IsWeekend(DateOnly date)
    {
        return date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;
    }
}