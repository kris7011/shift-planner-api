using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;

namespace ShiftPlanner.Api.IntegrationTests;

/// <summary>
/// Hosts the real API in memory against a private, shared-cache in-memory SQLite database.
/// EF Core migrations run on startup exactly as in the application.
/// The open connection keeps the database alive for the lifetime of the factory.
/// </summary>
public sealed class ShiftPlannerApiFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _keepAliveConnection = new(
        $"Data Source=shiftplanner-tests-{Guid.NewGuid():N};Mode=Memory;Cache=Shared");

    public ShiftPlannerApiFactory()
    {
        _keepAliveConnection.Open();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:ShiftPlanner", _keepAliveConnection.ConnectionString);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
        {
            _keepAliveConnection.Dispose();
        }
    }
}
