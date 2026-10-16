using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace ShiftPlanner.Api.IntegrationTests;

public sealed class DemoAndScheduleEndpointTests : ApiTestBase
{
    [Fact]
    public async Task Health_ReturnsHealthy()
    {
        var health = await GetJsonAsync("/health");

        Assert.Equal("Healthy", health.GetProperty("status").GetString());
    }

    [Fact]
    public async Task DemoReset_SeedsEmployeesAndShifts_AndSeedSkipsWhenDataExists()
    {
        var resetResponse = await Client.PostAsync("/api/demo/reset", content: null);
        var reset = await resetResponse.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.OK, resetResponse.StatusCode);
        Assert.True(reset.GetProperty("wasSeeded").GetBoolean());
        Assert.Equal(10, (await GetJsonAsync("/api/employees")).GetArrayLength());
        Assert.Equal(14, (await GetJsonAsync("/api/shifts")).GetArrayLength());

        var seed = await (await Client.PostAsync("/api/demo/seed", content: null))
            .Content.ReadFromJsonAsync<JsonElement>();

        Assert.False(seed.GetProperty("wasSeeded").GetBoolean());
        Assert.Equal(14, (await GetJsonAsync("/api/shifts")).GetArrayLength());
    }

    [Fact]
    public async Task Shifts_SerializeShiftTypeAsString()
    {
        await ResetDemoDataAsync();

        var shifts = await GetJsonAsync("/api/shifts");

        // The frontend compares shift types with "Day", "Evening", "Night" and "OnCall".
        Assert.All(
            shifts.EnumerateArray(),
            shift => Assert.Contains(shift.GetProperty("shiftType").GetString(), new[] { "Day", "Evening", "Night", "OnCall" }));
    }

    [Fact]
    public async Task ScheduleOverview_ReportsDemoSkillGaps()
    {
        await ResetDemoDataAsync();

        var overview = await GetJsonAsync("/api/schedule/overview");

        Assert.Equal(14, overview.GetProperty("totalShifts").GetInt32());
        Assert.Equal(10, overview.GetProperty("employeeCount").GetInt32());

        var uncoveredSkills = overview.GetProperty("uncoveredRequiredSkills").EnumerateArray()
            .Select(skill => skill.GetProperty("skill").GetString())
            .ToList();

        // The demo data deliberately contains shifts requiring skills no employee has.
        Assert.Contains("UL", uncoveredSkills);
        Assert.Contains("Intervention", uncoveredSkills);
    }

    [Fact]
    public async Task GenerateSchedule_PersistsAssignmentsForOpenShifts()
    {
        await ResetDemoDataAsync();
        var shiftsBefore = await GetJsonAsync("/api/shifts");
        var assignedBefore = CountAssigned(shiftsBefore);

        var response = await Client.PostAsJsonAsync("/api/schedule/generate", new { maxAssignmentsPerEmployee = 5 });
        var result = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var assignments = result.GetProperty("assignments").EnumerateArray().ToList();
        Assert.Equal(shiftsBefore.GetArrayLength() - assignedBefore, assignments.Count);

        var newlyAssigned = assignments.Count(assignment => assignment.GetProperty("wasAssigned").GetBoolean());
        Assert.True(newlyAssigned > 0);
        Assert.All(
            assignments.Where(assignment => !assignment.GetProperty("wasAssigned").GetBoolean()),
            assignment => Assert.NotEmpty(assignment.GetProperty("failureReasons").EnumerateArray()));

        var shiftsAfter = await GetJsonAsync("/api/shifts");
        Assert.Equal(assignedBefore + newlyAssigned, CountAssigned(shiftsAfter));
    }

    [Fact]
    public async Task GenerateSchedule_ReturnsBadRequestWithError_WhenAssignmentLimitIsNotPositive()
    {
        var response = await Client.PostAsJsonAsync("/api/schedule/generate", new { maxAssignmentsPerEmployee = 0 });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("MaxAssignmentsPerEmployee must be greater than 0.", await ReadErrorAsync(response));
    }

    [Fact]
    public async Task Simulate_ReportsUncoverableShift_ForSkillNoEmployeeHas()
    {
        await ResetDemoDataAsync();

        var simulation = await SimulateAsync(requiredSkill: "UL");

        Assert.False(simulation.GetProperty("canBeCovered").GetBoolean());
        Assert.Equal("High", simulation.GetProperty("riskLevel").GetString());
        Assert.Equal(JsonValueKind.Null, simulation.GetProperty("suggestedEmployeeId").ValueKind);
    }

    [Fact]
    public async Task Simulate_SuggestsEmployee_ForCoverableShift_WithoutPersistingIt()
    {
        await ResetDemoDataAsync();

        var simulation = await SimulateAsync(requiredSkill: "CT");

        Assert.True(simulation.GetProperty("canBeCovered").GetBoolean());
        Assert.False(string.IsNullOrEmpty(simulation.GetProperty("suggestedEmployeeName").GetString()));
        Assert.Equal(14, (await GetJsonAsync("/api/shifts")).GetArrayLength());
    }

    [Theory]
    [InlineData("", 1, 5, "RequiredSkill is required.")]
    [InlineData("CT", 0, 5, "RequiredStaff must be greater than 0.")]
    [InlineData("CT", 1, 0, "MaxAssignmentsPerEmployee must be greater than 0.")]
    public async Task Simulate_ReturnsBadRequestWithError_WhenRequestIsInvalid(
        string requiredSkill,
        int requiredStaff,
        int maxAssignmentsPerEmployee,
        string expectedError)
    {
        var response = await Client.PostAsJsonAsync("/api/schedule/simulate", new
        {
            date = "2026-05-15",
            shiftType = "Day",
            requiredSkill,
            requiredStaff,
            maxAssignmentsPerEmployee
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(expectedError, await ReadErrorAsync(response));
    }

    private async Task<JsonElement> SimulateAsync(string requiredSkill)
    {
        var response = await Client.PostAsJsonAsync("/api/schedule/simulate", new
        {
            date = "2026-05-15",
            shiftType = "Day",
            requiredSkill,
            requiredStaff = 1,
            maxAssignmentsPerEmployee = 5
        });

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<JsonElement>();
    }

    private static int CountAssigned(JsonElement shifts)
    {
        return shifts.EnumerateArray()
            .Count(shift => shift.GetProperty("employeeId").ValueKind != JsonValueKind.Null);
    }
}
