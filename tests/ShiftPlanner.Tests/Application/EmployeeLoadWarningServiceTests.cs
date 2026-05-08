using ShiftPlanner.Application.Employees;

namespace ShiftPlanner.Tests.Application;

public class EmployeeLoadWarningServiceTests
{
    [Theory]
    [InlineData(11, 10, true)]
    [InlineData(10, 10, false)]
    [InlineData(5, 10, false)]
    public void HasHighLoad_ReturnsCorrectWarningStatus(
        int totalLoad,
        int threshold,
        bool expected)
    {
        var service = new EmployeeLoadWarningService();

        var result = service.HasHighLoad(totalLoad, threshold);

        Assert.Equal(expected, result);
    }
}