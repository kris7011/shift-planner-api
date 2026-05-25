using ShiftPlanner.Application.Employees;
using Microsoft.Extensions.DependencyInjection;
using ShiftPlanner.Application.Scheduling;
using ShiftPlanner.Application.Scheduling.Rules;
using ShiftPlanner.Application.Scheduling.Overview;
using ShiftPlanner.Application.Scheduling.Simulation;
using ShiftPlanner.Application.Demo;
using ShiftPlanner.Application.Scheduling.Analysis;

namespace ShiftPlanner.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IEmployeeLoadService, EmployeeLoadService>();
        services.AddScoped<IEmployeeLoadWarningService, EmployeeLoadWarningService>();
        services.AddScoped<IEmployeeLoadStatusService, EmployeeLoadStatusService>();
        services.AddScoped<EmployeeLoadAnalysisService>();
        services.AddScoped<EmployeeLoadOverviewService>();
        services.AddScoped<EmployeeLoadDetailsService>();
        services.AddScoped<IScheduleGeneratorService, ScheduleGeneratorService>();
        services.AddScoped<ISchedulingRule, MaxAssignmentsRule>();
        services.AddScoped<ISchedulingRule, SameDayShiftRule>();
        services.AddScoped<ISchedulingRule, NightToDayRule>();
        services.AddScoped<ISchedulingRule, HighLoadRule>();
        services.AddScoped<IScheduleOverviewService, ScheduleOverviewService>();
        services.AddScoped<IScheduleSimulationService, ScheduleSimulationService>();
        services.AddScoped<ShiftAssignmentAnalysisService>();
        services.AddScoped<IDemoDataService, DemoDataService>();

        return services;
    }
}