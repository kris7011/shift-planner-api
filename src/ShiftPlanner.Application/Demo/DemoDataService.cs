using ShiftPlanner.Application.Employees;
using ShiftPlanner.Application.Shifts;

namespace ShiftPlanner.Application.Demo;

public class DemoDataService : IDemoDataService
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IShiftRepository _shiftRepository;

    public DemoDataService(
        IEmployeeRepository employeeRepository,
        IShiftRepository shiftRepository)
    {
        _employeeRepository = employeeRepository;
        _shiftRepository = shiftRepository;
    }

    public async Task SeedAsync()
    {
        var employees = await _employeeRepository.GetAllAsync();
        var shifts = await _shiftRepository.GetAllAsync();

        if (employees.Count > 0 || shifts.Count > 0)
        {
            return;
        }

        // Demo data will be added in the next step.
    }
}