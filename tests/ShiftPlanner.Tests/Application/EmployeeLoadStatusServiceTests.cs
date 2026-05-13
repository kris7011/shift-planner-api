using ShiftPlanner.Application.Employees;
using ShiftPlanner.Domain.Employees;

namespace ShiftPlanner.Tests.Application;

public class EmployeeLoadStatusServiceTests
{
    [Theory]
    [InlineData(0, LoadStatus.Low)]
    [InlineData(3, LoadStatus.Low)]
    [InlineData(4, LoadStatus.Medium)]
    [InlineData(7, LoadStatus.Medium)]
    [InlineData(8, LoadStatus.High)]
    [InlineData(15, LoadStatus.High)]
    public void CalculateStatus_ReturnsCorrectStatus(
        int totalLoad,
        LoadStatus expectedStatus)
    {
        var service = new EmployeeLoadStatusService();

        var result = service.CalculateStatus(totalLoad);

        Assert.Equal(expectedStatus, result);
    }
}