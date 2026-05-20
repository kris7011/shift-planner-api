using ShiftPlanner.Application.Employees;
using ShiftPlanner.Application.Shifts;
using ShiftPlanner.Domain.Employees;
using ShiftPlanner.Domain.Shifts;

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

    public async Task<DemoSeedResult> SeedAsync()
    {
        var employees = await _employeeRepository.GetAllAsync();
        var shifts = await _shiftRepository.GetAllAsync();

        if (employees.Count > 0 || shifts.Count > 0)
        {
            return new DemoSeedResult
            {
                WasSeeded = false,
                Message = "Demo data was skipped because the database already contains data.",
                EmployeeCount = employees.Count,
                ShiftCount = shifts.Count
            };
        }

        var demoEmployees = CreateDemoEmployees();

        foreach (var employee in demoEmployees)
        {
            await _employeeRepository.AddAsync(employee);
        }

        var demoShifts = CreateDemoShifts(demoEmployees);

        foreach (var shift in demoShifts)
        {
            await _shiftRepository.CreateAsync(shift);
        }

        return new DemoSeedResult
        {
            WasSeeded = true,
            Message = "Demo data was seeded.",
            EmployeeCount = demoEmployees.Count,
            ShiftCount = demoShifts.Count
        };
    }

    private static List<Employee> CreateDemoEmployees()
    {
        return
        [
            new Employee("Kris", new List<string> { "CT", "XR" }),
            new Employee("Mette", new List<string> { "MRI", "XR" }),
            new Employee("Henrik", new List<string> { "CT", "Night" }),
            new Employee("Sofie", new List<string> { "XR", "Night" }),
            new Employee("Lars", new List<string> { "MRI", "CT" }),
            new Employee("Anna", new List<string> { "XR" }),
            new Employee("Jonas", new List<string> { "CT" }),
            new Employee("Maria", new List<string> { "MRI", "Night" }),
            new Employee("Peter", new List<string> { "XR", "CT" }),
            new Employee("Line", new List<string> { "MRI" })
        ];
    }

    private static List<Shift> CreateDemoShifts(List<Employee> employees)
    {
        var kris = employees.First(employee => employee.Name == "Kris");
        var mette = employees.First(employee => employee.Name == "Mette");
        var henrik = employees.First(employee => employee.Name == "Henrik");
        var sofie = employees.First(employee => employee.Name == "Sofie");

        return
        [
            new Shift(
                new DateOnly(2026, 5, 11),
                ShiftType.Day,
                "CT",
                1,
                kris.Id),

            new Shift(
                new DateOnly(2026, 5, 11),
                ShiftType.Evening,
                "MRI",
                1,
                mette.Id),

            new Shift(
                new DateOnly(2026, 5, 12),
                ShiftType.Night,
                "Night",
                1,
                henrik.Id),

            new Shift(
                new DateOnly(2026, 5, 12),
                ShiftType.Day,
                "XR",
                1,
                sofie.Id),

            new Shift(
                new DateOnly(2026, 5, 13),
                ShiftType.Day,
                "CT",
                1),

            new Shift(
                new DateOnly(2026, 5, 13),
                ShiftType.Evening,
                "MRI",
                1),

            new Shift(
                new DateOnly(2026, 5, 14),
                ShiftType.Night,
                "Night",
                1),

            new Shift(
                new DateOnly(2026, 5, 14),
                ShiftType.Day,
                "XR",
                1),

            new Shift(
                new DateOnly(2026, 5, 15),
                ShiftType.Day,
                "UL",
                1),

            new Shift(
                new DateOnly(2026, 5, 15),
                ShiftType.Evening,
                "Intervention",
                1)
        ];
    }
}