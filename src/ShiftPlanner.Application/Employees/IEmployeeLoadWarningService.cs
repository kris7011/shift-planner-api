namespace ShiftPlanner.Application.Employees;

public interface IEmployeeLoadWarningService
{
    bool HasHighLoad(int totalLoad, int threshold);
}