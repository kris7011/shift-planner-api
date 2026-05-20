using ShiftPlanner.Application.Demo;

namespace ShiftPlanner.Api.Demo;

public static class DemoEndpoints
{
    public static void MapDemoEndpoints(this WebApplication app)
    {
        app.MapPost("/api/demo/seed", async (IDemoDataService demoDataService) =>
        {
            await demoDataService.SeedAsync();

            return Results.Ok(new
            {
                Message = "Demo data seed completed."
            });
        })
        .WithName("SeedDemoData")
        .WithSummary("Seeds demo data")
        .WithDescription("Creates demo employees and shifts if the database is empty.")
        .Produces(StatusCodes.Status200OK);
    }
}