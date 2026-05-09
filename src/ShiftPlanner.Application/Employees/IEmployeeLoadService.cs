using ShiftPlanner.Domain.Employees;
using ShiftPlanner.Domain.Shifts;

namespace ShiftPlanner.Application.Employees;

public interface IEmployeeLoadService
{
    int CalculateTotalLoad(Employee employee, List<Shift> shifts);
    int CalculateLoad(List<Shift> shifts);
}