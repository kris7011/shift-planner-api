using System.Text.Json.Serialization;
using ShiftPlanner.Api.LoadAnalysis;
using ShiftPlanner.Api.Health;
using ShiftPlanner.Api.Employees;
using ShiftPlanner.Api.Middleware;
using ShiftPlanner.Api.Shifts;
using ShiftPlanner.Api.Scheduling;
using ShiftPlanner.Application;
using ShiftPlanner.Infrastructure;
using ShiftPlanner.Api.Demo;
using Microsoft.EntityFrameworkCore;
using ShiftPlanner.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy
            .WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ShiftPlannerDbContext>();
    dbContext.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("FrontendPolicy");
app.UseGlobalExceptionHandling();
app.UseHttpsRedirection();

app.MapLoadAnalysisEndpoints();
app.MapHealthEndpoints();
app.MapEmployeeEndpoints();
app.MapShiftEndpoints();
app.MapSchedulingEndpoints();
app.MapDemoEndpoints();

app.Run();