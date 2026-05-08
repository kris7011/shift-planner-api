using ShiftPlanner.Application.Employees;
using Microsoft.Extensions.DependencyInjection;

namespace ShiftPlanner.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IEmployeeLoadService, EmployeeLoadService>();
        services.AddScoped<IEmployeeLoadWarningService, EmployeeLoadWarningService>();
        services.AddScoped<EmployeeLoadAnalysisService>();

        return services;
    }
}