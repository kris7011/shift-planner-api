namespace ShiftPlanner.Api.Health;

public static class HealthEndpoints
{
    public static void MapHealthEndpoints(this WebApplication app)
    {
        app.MapGet("/health", () =>
        {
            return Results.Ok(new
            {
                status = "Healthy",
                service = "ShiftPlanner.Api",
                timestamp = DateTime.UtcNow
            });
        });
    }
}