using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ShiftPlanner.Application.Employees;
using ShiftPlanner.Infrastructure.Persistence.Employees;
using ShiftPlanner.Application.Shifts;
using ShiftPlanner.Infrastructure.Persistence;
using ShiftPlanner.Infrastructure.Persistence.Shifts;

namespace ShiftPlanner.Infrastructure;

public static class DependencyInjection
{
    private const string DefaultConnectionString = "Data Source=shiftplanner.db";

    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddDbContext<ShiftPlannerDbContext>((serviceProvider, options) =>
        {
            var connectionString = serviceProvider
                .GetRequiredService<IConfiguration>()
                .GetConnectionString("ShiftPlanner");

            options.UseSqlite(string.IsNullOrWhiteSpace(connectionString)
                ? DefaultConnectionString
                : connectionString);
        });

        services.AddScoped<IEmployeeRepository, EmployeeRepository>();
        services.AddScoped<IShiftRepository, ShiftRepository>();
        services.AddScoped<IEmployeeLoadStatusService, EmployeeLoadStatusService>();


        return services;
    }
}