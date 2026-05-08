using ShiftPlanner.Domain.Employees;

namespace ShiftPlanner.Application.Employees;

public interface IEmployeeRepository
{
    Task<Employee> AddAsync(Employee employee);
    Task<List<Employee>> GetAllAsync();
}