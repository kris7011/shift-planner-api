using ShiftPlanner.Application.Scheduling;
using ShiftPlanner.Domain.Employees;
using ShiftPlanner.Domain.Shifts;
using ShiftPlanner.Application.Employees;
using ShiftPlanner.Application.Scheduling.Rules;
using ShiftPlanner.Tests.Helpers;

namespace ShiftPlanner.Tests.Application;

public class ScheduleGeneratorServiceTests
{
    [Fact]
    public void Generate_AssignsEmployee_WhenSkillMatches()
    {
        var employee = new Employee("Kris", new List<string> { "CT" });

        var shift = new Shift(
            new DateOnly(2026, 5, 20),
            ShiftType.Day,
            "CT",
            1);

        var employees = new List<Employee> { employee };
        var shifts = new List<Shift> { shift };

        var loadService = new EmployeeLoadService();
        var statusService = new EmployeeLoadStatusService();

        var rules = new List<ISchedulingRule>
        {
            new MaxAssignmentsRule(),
            new SameDayShiftRule(),
            new NightToDayRule(),
            new HighLoadRule(loadService, statusService)
        };

        var service = ScheduleGeneratorServiceFactory.Create();

        var result = service.Generate(
            employees,
            openShifts: shifts,
            existingShifts: new List<Shift>(),
            maxAssignmentsPerEmployee: 5);

        Assert.Single(result);

        Assert.True(result[0].WasAssigned);
        Assert.Equal(employee.Id, result[0].EmployeeId);
    }

    [Fact]
    public void Generate_DoesNotAssignEmployee_WhenSkillDoesNotMatch()
    {
        var employee = new Employee("Kris", new List<string> { "MRI" });

        var shift = new Shift(
            new DateOnly(2026, 5, 20),
            ShiftType.Day,
            "CT",
            1);

        var employees = new List<Employee> { employee };
        var shifts = new List<Shift> { shift };

        var loadService = new EmployeeLoadService();
        var statusService = new EmployeeLoadStatusService();

        var rules = new List<ISchedulingRule>
        {
            new MaxAssignmentsRule(),
            new SameDayShiftRule(),
            new NightToDayRule(),
            new HighLoadRule(loadService, statusService)
        };

        var service = ScheduleGeneratorServiceFactory.Create();

        var result = service.Generate(
            employees,
            openShifts: shifts,
            existingShifts: new List<Shift>(),
            maxAssignmentsPerEmployee: 5);

        Assert.Single(result);

        Assert.False(result[0].WasAssigned);
        Assert.Null(result[0].EmployeeId);
    }

    [Fact]
    public void Generate_AssignsEmployeeWithLowestCurrentLoad_WhenMultipleEmployeesMatch()
    {
        var highLoadEmployee = new Employee("Kris", new List<string> { "CT" });
        var lowLoadEmployee = new Employee("Mette", new List<string> { "CT" });

        var existingNightShift = new Shift(
            new DateOnly(2026, 5, 18),
            ShiftType.Night,
            "CT",
            1,
            highLoadEmployee.Id);

        var openShift = new Shift(
            new DateOnly(2026, 5, 20),
            ShiftType.Day,
            "CT",
            1);

        var employees = new List<Employee>
        {
            highLoadEmployee,
            lowLoadEmployee
        };

        var openShifts = new List<Shift> { openShift };
        var existingShifts = new List<Shift> { existingNightShift };

        var loadService = new EmployeeLoadService();
        var statusService = new EmployeeLoadStatusService();
        var rules = new List<ISchedulingRule>
        {
            new MaxAssignmentsRule(),
            new SameDayShiftRule(),
            new NightToDayRule(),
            new HighLoadRule(loadService, statusService)
        };

        var service = ScheduleGeneratorServiceFactory.Create();

        var result = service.Generate(
            employees,
            openShifts,
            existingShifts,
            maxAssignmentsPerEmployee: 5);

        Assert.Single(result);
        Assert.True(result[0].WasAssigned);
        Assert.Equal(lowLoadEmployee.Id, result[0].EmployeeId);
    }

    [Fact]
    public void Generate_DoesNotAssignEmployee_WhenProjectedLoadWouldBeHigh()
    {
        var employee = new Employee("Kris", new List<string> { "CT" });

        var existingNightShift = new Shift(
            new DateOnly(2026, 5, 18),
            ShiftType.Night,
            "CT",
            1,
            employee.Id);

        var newNightShift = new Shift(
            new DateOnly(2026, 5, 20),
            ShiftType.Night,
            "CT",
            1);

        var employees = new List<Employee> { employee };
        var shifts = new List<Shift> { existingNightShift, newNightShift };

        var loadService = new EmployeeLoadService();
        var statusService = new EmployeeLoadStatusService();
        var rules = new List<ISchedulingRule>
        {
            new MaxAssignmentsRule(),
            new SameDayShiftRule(),
            new NightToDayRule(),
            new HighLoadRule(loadService, statusService)
        };

        var service = ScheduleGeneratorServiceFactory.Create();

        var result = service.Generate(
            employees,
            openShifts: shifts,
            existingShifts: new List<Shift>(),
            maxAssignmentsPerEmployee: 5);

        var assignment = result.First(x => x.ShiftId == newNightShift.Id);

        Assert.False(assignment.WasAssigned);
        Assert.Null(assignment.EmployeeId);
    }

    [Fact]
    public void Generate_DoesNotAssignEmployee_ToMultipleShiftsSameDay()
    {
        var employee = new Employee("Kris", new List<string> { "CT" });

        var existingShift = new Shift(
            new DateOnly(2026, 5, 20),
            ShiftType.Day,
            "CT",
            1,
            employee.Id);

        var openShift = new Shift(
            new DateOnly(2026, 5, 20),
            ShiftType.Evening,
            "CT",
            1);

        var employees = new List<Employee> { employee };
        var openShifts = new List<Shift> { openShift };
        var existingShifts = new List<Shift> { existingShift };

        var loadService = new EmployeeLoadService();
        var statusService = new EmployeeLoadStatusService();
        var rules = new List<ISchedulingRule>
        {
            new MaxAssignmentsRule(),
            new SameDayShiftRule(),
            new NightToDayRule(),
            new HighLoadRule(loadService, statusService)
        };

        var service = ScheduleGeneratorServiceFactory.Create();

        var result = service.Generate(
            employees,
            openShifts,
            existingShifts,
            maxAssignmentsPerEmployee: 5);

        Assert.Single(result);
        Assert.False(result[0].WasAssigned);
        Assert.Null(result[0].EmployeeId);
    }

    [Fact]
    public void Generate_DoesNotAssignDayShift_AfterNightShift()
    {
        var employee = new Employee("Kris", new List<string> { "CT" });

        var existingNightShift = new Shift(
            new DateOnly(2026, 5, 20),
            ShiftType.Night,
            "CT",
            1,
            employee.Id);

        var openDayShift = new Shift(
            new DateOnly(2026, 5, 21),
            ShiftType.Day,
            "CT",
            1);

        var employees = new List<Employee> { employee };
        var openShifts = new List<Shift> { openDayShift };
        var existingShifts = new List<Shift> { existingNightShift };

        var loadService = new EmployeeLoadService();
        var statusService = new EmployeeLoadStatusService();
        var rules = new List<ISchedulingRule>
        {
            new MaxAssignmentsRule(),
            new SameDayShiftRule(),
            new NightToDayRule(),
            new HighLoadRule(loadService, statusService)
        };

        var service = ScheduleGeneratorServiceFactory.Create();

        var result = service.Generate(
            employees,
            openShifts,
            existingShifts,
            maxAssignmentsPerEmployee: 5);

        Assert.Single(result);
        Assert.False(result[0].WasAssigned);
        Assert.Null(result[0].EmployeeId);
    }

    [Fact]
    public void Generate_ReturnsFailureReasons_WhenEmployeeCannotBeAssigned()
    {
        var employee = new Employee("Kris", new List<string> { "CT" });

        var existingShift = new Shift(
            new DateOnly(2026, 5, 20),
            ShiftType.Day,
            "CT",
            1,
            employee.Id);

        var openShift = new Shift(
            new DateOnly(2026, 5, 20),
            ShiftType.Evening,
            "CT",
            1);

        var employees = new List<Employee> { employee };

        var openShifts = new List<Shift> { openShift };

        var existingShifts = new List<Shift> { existingShift };

        var loadService = new EmployeeLoadService();
        var statusService = new EmployeeLoadStatusService();

        var rules = new List<ISchedulingRule>
    {
        new MaxAssignmentsRule(),
        new SameDayShiftRule(),
        new NightToDayRule(),
        new HighLoadRule(loadService, statusService)
    };

        var service = ScheduleGeneratorServiceFactory.Create();

        var result = service.Generate(
            employees,
            openShifts,
            existingShifts,
            maxAssignmentsPerEmployee: 5);

        var assignment = result.First();

        Assert.False(assignment.WasAssigned);

        Assert.Contains(
            assignment.FailureReasons,
            x => x.Contains("same day"));
    }
}