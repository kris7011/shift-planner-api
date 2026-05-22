using ShiftPlanner.Application.Demo;
using ShiftPlanner.Application.Employees;
using ShiftPlanner.Application.Shifts;
using ShiftPlanner.Domain.Employees;
using ShiftPlanner.Domain.Shifts;

namespace ShiftPlanner.Tests.Application.Demo;

public class DemoDataServiceTests
{
    [Fact]
    public async Task ResetAsync_ReturnsSeededResult_WithExpectedCounts()
    {
        var employeeRepository = new FakeEmployeeRepository();
        var shiftRepository = new FakeShiftRepository();

        var service = new DemoDataService(
            employeeRepository,
            shiftRepository);

        var result = await service.ResetAsync();

        Assert.True(result.WasSeeded);
        Assert.Equal("Demo data was reset and seeded.", result.Message);
        Assert.Equal(10, result.EmployeeCount);
        Assert.Equal(14, result.ShiftCount);

        var employees = await employeeRepository.GetAllAsync();
        var shifts = await shiftRepository.GetAllAsync();

        Assert.Equal(10, employees.Count);
        Assert.Equal(14, shifts.Count);
    }

    private class FakeEmployeeRepository : IEmployeeRepository
    {
        private readonly List<Employee> _employees = [];

        public Task<Employee> AddAsync(Employee employee)
        {
            _employees.Add(employee);

            return Task.FromResult(employee);
        }

        public Task<List<Employee>> GetAllAsync()
        {
            return Task.FromResult(_employees);
        }

        public Task DeleteAllAsync()
        {
            _employees.Clear();

            return Task.CompletedTask;
        }
    }

    private class FakeShiftRepository : IShiftRepository
    {
        private readonly List<Shift> _shifts = [];

        public Task<Shift> CreateAsync(Shift shift)
        {
            _shifts.Add(shift);

            return Task.FromResult(shift);
        }

        public Task<Shift> UpdateAsync(Shift shift)
        {
            return Task.FromResult(shift);
        }

        public Task<List<Shift>> GetAllAsync()
        {
            return Task.FromResult(_shifts);
        }

        public Task<List<Shift>> GetByEmployeeIdAsync(Guid employeeId)
        {
            var shifts = _shifts
                .Where(shift => shift.EmployeeId == employeeId)
                .ToList();

            return Task.FromResult(shifts);
        }

        public Task DeleteAllAsync()
        {
            _shifts.Clear();

            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task SeedAsync_ReturnsSkippedResult_WhenDataAlreadyExists()
    {
        var employeeRepository = new FakeEmployeeRepository();
        var shiftRepository = new FakeShiftRepository();

        await employeeRepository.AddAsync(
            new Employee("Existing Employee", new List<string> { "CT" }));

        var service = new DemoDataService(
            employeeRepository,
            shiftRepository);

        var result = await service.SeedAsync();

        Assert.False(result.WasSeeded);
        Assert.Equal(
            "Demo data was skipped because the database already contains data.",
            result.Message);
        Assert.Equal(1, result.EmployeeCount);
        Assert.Equal(0, result.ShiftCount);

        var employees = await employeeRepository.GetAllAsync();
        var shifts = await shiftRepository.GetAllAsync();

        Assert.Single(employees);
        Assert.Empty(shifts);
    }
}