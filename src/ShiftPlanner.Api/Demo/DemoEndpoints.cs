using ShiftPlanner.Application.Demo;

namespace ShiftPlanner.Api.Demo;

public static class DemoEndpoints
{
    public static void MapDemoEndpoints(this WebApplication app)
    {
        app.MapPost("/api/demo/seed", async (IDemoDataService demoDataService) =>
        {
            var result = await demoDataService.SeedAsync();

            return Results.Ok(result);
        })
        .WithName("SeedDemoData")
        .WithSummary("Seeds demo data")
        .WithDescription("Creates demo employees and shifts if the database is empty.")
        .Produces(StatusCodes.Status200OK);

        app.MapPost("/api/demo/reset", async (IDemoDataService demoDataService) =>
        {
            var result = await demoDataService.ResetAsync();

            return Results.Ok(result);
        })
        .WithName("ResetDemoData")
        .WithSummary("Resets demo data")
        .WithDescription("Deletes all employees and shifts, then creates fresh demo employees and shifts.")
        .Produces<DemoSeedResult>(StatusCodes.Status200OK);
    }
}