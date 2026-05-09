using ShiftPlanner.Domain.Employees;
using ShiftPlanner.Domain.Shifts;

namespace ShiftPlanner.Application.Employees;

public class EmployeeLoadService : IEmployeeLoadService
{
    private readonly ShiftLoadCalculator _shiftLoadCalculator;

    public EmployeeLoadService()
    {
        _shiftLoadCalculator = new ShiftLoadCalculator();
    }

    public int CalculateTotalLoad(Employee employee, List<Shift> shifts)
    {
        var total = 0;

        foreach (var shift in shifts)
        {
            if (shift.AssignedEmployees.Contains(employee))
            {
                total += _shiftLoadCalculator.Calculate(shift);
            }
        }

        return total;
    }

    public int CalculateLoad(List<Shift> shifts)
    {
        var total = 0;

        foreach (var shift in shifts)
        {
            total += _shiftLoadCalculator.Calculate(shift);
        }

        return total;
    }
}