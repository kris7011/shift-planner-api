using ShiftPlanner.Domain.Employees;

namespace ShiftPlanner.Application.Employees;

public class EmployeeLoadStatusService : IEmployeeLoadStatusService
{
    public LoadStatus CalculateStatus(int totalLoad)
    {
        if (totalLoad >= 8)
        {
            return LoadStatus.High;
        }

        if (totalLoad >= 4)
        {
            return LoadStatus.Medium;
        }

        return LoadStatus.Low;
    }
}