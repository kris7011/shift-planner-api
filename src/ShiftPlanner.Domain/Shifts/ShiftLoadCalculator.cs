namespace ShiftPlanner.Domain.Shifts;

public class ShiftLoadCalculator
{
    public int Calculate(Shift shift)
    {
        return shift.ShiftType switch
        {
            ShiftType.Day => 1,
            ShiftType.Evening => 2,
            ShiftType.Night => 4,
            ShiftType.OnCall => 3,
            _ => 0
        };
    }
}