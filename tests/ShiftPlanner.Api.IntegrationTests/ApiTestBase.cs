using System.Net.Http.Json;
using System.Text.Json;

namespace ShiftPlanner.Api.IntegrationTests;

/// <summary>
/// Gives every test its own API instance and empty database.
/// </summary>
public abstract class ApiTestBase : IDisposable
{
    private readonly ShiftPlannerApiFactory _factory = new();

    protected ApiTestBase()
    {
        Client = _factory.CreateClient();
    }

    protected HttpClient Client { get; }

    public void Dispose()
    {
        Client.Dispose();
        _factory.Dispose();
        GC.SuppressFinalize(this);
    }

    protected async Task ResetDemoDataAsync()
    {
        var response = await Client.PostAsync("/api/demo/reset", content: null);
        response.EnsureSuccessStatusCode();
    }

    protected Task<JsonElement> GetJsonAsync(string path)
    {
        return Client.GetFromJsonAsync<JsonElement>(path);
    }

    protected static async Task<string?> ReadErrorAsync(HttpResponseMessage response)
    {
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();

        return body.GetProperty("error").GetString();
    }
}
