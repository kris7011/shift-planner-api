using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ShiftPlanner.Infrastructure.Persistence;

namespace ShiftPlanner.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddDbContext<ShiftPlannerDbContext>(options =>
        {
            options.UseSqlite("Data Source=shiftplanner.db");
        });

        return services;
    }
}