using ShiftPlanner.Domain.Employees;
using ShiftPlanner.Domain.Shifts;

namespace ShiftPlanner.Application.Employees;

public class EmployeeLoadAnalysisService
{
    private readonly IEmployeeLoadService _employeeLoadService;
    private readonly IEmployeeLoadWarningService _warningService;

    public EmployeeLoadAnalysisService(
        IEmployeeLoadService employeeLoadService,
        IEmployeeLoadWarningService warningService)
    {
        _employeeLoadService = employeeLoadService;
        _warningService = warningService;
    }

    public EmployeeLoadAnalysisResult Analyze(Employee employee, List<Shift> shifts, int threshold)
    {
        var totalLoad = _employeeLoadService.CalculateTotalLoad(employee, shifts);

        return new EmployeeLoadAnalysisResult
        {
            EmployeeId = employee.Id,
            EmployeeName = employee.Name,
            TotalLoad = totalLoad,
            Threshold = threshold,
            HasHighLoad = _warningService.HasHighLoad(totalLoad, threshold)
        };
    }
}