using ShiftPlanner.Api.LoadAnalysis;
using ShiftPlanner.Api.Health;
using ShiftPlanner.Api.Employees;
using ShiftPlanner.Api.Middleware;
using ShiftPlanner.Application;
using ShiftPlanner.Infrastructure;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddApplication();
builder.Services.AddInfrastructure();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseGlobalExceptionHandling();
app.UseHttpsRedirection();

app.MapLoadAnalysisEndpoints();
app.MapHealthEndpoints();
app.MapEmployeeEndpoints();

app.Run();