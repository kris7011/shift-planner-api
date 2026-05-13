using ShiftPlanner.Application.Employees;
using Microsoft.Extensions.DependencyInjection;
using ShiftPlanner.Application.Scheduling;

namespace ShiftPlanner.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IEmployeeLoadService, EmployeeLoadService>();
        services.AddScoped<IEmployeeLoadWarningService, EmployeeLoadWarningService>();
        services.AddScoped<EmployeeLoadAnalysisService>();
        services.AddScoped<IScheduleGeneratorService, ScheduleGeneratorService>();

        return services;
    }
}