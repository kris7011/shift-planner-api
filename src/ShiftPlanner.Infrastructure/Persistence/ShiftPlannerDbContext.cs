using Microsoft.EntityFrameworkCore;
using ShiftPlanner.Infrastructure.Persistence.Employees;

namespace ShiftPlanner.Infrastructure.Persistence;

public class ShiftPlannerDbContext : DbContext
{
    public ShiftPlannerDbContext(DbContextOptions<ShiftPlannerDbContext> options)
        : base(options)
    {
    }

    public DbSet<EmployeeEntity> Employees => Set<EmployeeEntity>();
}