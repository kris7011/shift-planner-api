namespace ShiftPlanner.Application.Employees;

public class EmployeeLoadWarningService : IEmployeeLoadWarningService
{
    public bool HasHighLoad(int totalLoad, int threshold)
    {
        return totalLoad > threshold;
    }
}