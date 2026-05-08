using ShiftPlanner.Domain.Shifts;

namespace ShiftPlanner.Tests.Domain;

public class ShiftLoadCalculatorTests
{
    [Theory]
    [InlineData(ShiftType.Day, 1)]
    [InlineData(ShiftType.Evening, 2)]
    [InlineData(ShiftType.Night, 4)]
    [InlineData(ShiftType.OnCall, 3)]
    public void Calculate_ReturnsCorrectLoadScore(ShiftType shiftType, int expectedScore)
    {
        var shift = new Shift(
            new DateOnly(2026, 5, 11),
            shiftType,
            "CT",
            1);

        var calculator = new ShiftLoadCalculator();

        var score = calculator.Calculate(shift);

        Assert.Equal(expectedScore, score);
    }
}