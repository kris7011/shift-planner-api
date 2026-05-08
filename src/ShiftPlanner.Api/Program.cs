using ShiftPlanner.Api.LoadAnalysis;
using ShiftPlanner.Api.Health;
using ShiftPlanner.Application;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddApplication();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapLoadAnalysisEndpoints();
app.MapHealthEndpoints();

app.Run();