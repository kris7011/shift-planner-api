using ShiftPlanner.Domain.Employees;

namespace ShiftPlanner.Application.Employees;

public interface IEmployeeLoadStatusService
{
    LoadStatus CalculateStatus(int totalLoad);
}