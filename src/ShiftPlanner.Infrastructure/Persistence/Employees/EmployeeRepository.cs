using Microsoft.EntityFrameworkCore;
using ShiftPlanner.Application.Employees;
using ShiftPlanner.Domain.Employees;

namespace ShiftPlanner.Infrastructure.Persistence.Employees;

public class EmployeeRepository : IEmployeeRepository
{
    private readonly ShiftPlannerDbContext _dbContext;

    public EmployeeRepository(ShiftPlannerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Employee> AddAsync(Employee employee)
    {
        var entity = new EmployeeEntity
        {
            Id = employee.Id,
            Name = employee.Name,
            Skills = string.Join(",", employee.Skills)
        };

        _dbContext.Employees.Add(entity);

        await _dbContext.SaveChangesAsync();

        return employee;
    }

    public async Task<List<Employee>> GetAllAsync()
    {
        var entities = await _dbContext.Employees
            .AsNoTracking()
            .ToListAsync();

        var employees = new List<Employee>();

        foreach (var entity in entities)
        {
            var skills = entity.Skills
                .Split(",", StringSplitOptions.RemoveEmptyEntries)
                .ToList();

            employees.Add(Employee.FromPersistence(entity.Id, entity.Name, skills));
        }

        return employees;
    }
}