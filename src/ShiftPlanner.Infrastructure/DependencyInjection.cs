using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ShiftPlanner.Application.Employees;
using ShiftPlanner.Infrastructure.Persistence.Employees;
using ShiftPlanner.Application.Shifts;
using ShiftPlanner.Infrastructure.Persistence;
using ShiftPlanner.Infrastructure.Persistence.Shifts;

namespace ShiftPlanner.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddDbContext<ShiftPlannerDbContext>(options =>
        {
            options.UseSqlite("Data Source=shiftplanner.db");
        });

        services.AddScoped<IEmployeeRepository, EmployeeRepository>();
        services.AddScoped<IShiftRepository, ShiftRepository>();


        return services;
    }
}