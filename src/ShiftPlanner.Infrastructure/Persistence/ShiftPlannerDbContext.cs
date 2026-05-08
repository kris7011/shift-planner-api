using Microsoft.EntityFrameworkCore;
using ShiftPlanner.Infrastructure.Persistence.Employees;
using ShiftPlanner.Infrastructure.Persistence.Shifts;

namespace ShiftPlanner.Infrastructure.Persistence;

public class ShiftPlannerDbContext : DbContext
{
    public ShiftPlannerDbContext(DbContextOptions<ShiftPlannerDbContext> options)
        : base(options)
    {
    }

    public DbSet<EmployeeEntity> Employees => Set<EmployeeEntity>();
    public DbSet<ShiftEntity> Shifts => Set<ShiftEntity>();
}