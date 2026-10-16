using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ShiftPlanner.Api.Shifts;

namespace ShiftPlanner.Api.IntegrationTests;

/// <summary>
/// A shift is staffed by exactly one employee (single EmployeeId, one assignment per shift in schedule
/// generation and coverage). The API must reject other RequiredStaff values instead of silently storing 1.
/// </summary>
public sealed class ShiftStaffingEndpointTests : ApiTestBase
{
    [Theory]
    [InlineData(0)]
    [InlineData(2)]
    [InlineData(3)]
    public async Task CreateShift_ReturnsBadRequest_AndStoresNothing_WhenRequiredStaffIsNotOne(int requiredStaff)
    {
        var response = await Client.PostAsJsonAsync("/api/shifts", new
        {
            date = "2026-05-14",
            shiftType = "Day",
            requiredSkill = "CT",
            requiredStaff
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(ShiftStaffingErrors.UnsupportedRequiredStaff, await ReadErrorAsync(response));
        Assert.Equal(0, (await GetJsonAsync("/api/shifts")).GetArrayLength());
    }

    [Fact]
    public async Task CreateShift_WithOneRequiredEmployee_RoundTripsThroughPersistence()
    {
        var response = await Client.PostAsJsonAsync("/api/shifts", new
        {
            date = "2026-05-14",
            shiftType = "Night",
            requiredSkill = "Night",
            requiredStaff = 1
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var shift = Assert.Single((await GetJsonAsync("/api/shifts")).EnumerateArray());
        Assert.Equal(1, shift.GetProperty("requiredStaff").GetInt32());
        Assert.Equal("Night", shift.GetProperty("shiftType").GetString());
        Assert.Equal(JsonValueKind.Null, shift.GetProperty("employeeId").ValueKind);
    }

    [Fact]
    public async Task CreateShift_DefaultsRequiredStaffToOne_WhenOmitted()
    {
        var response = await Client.PostAsJsonAsync("/api/shifts", new
        {
            date = "2026-05-14",
            shiftType = "Day",
            requiredSkill = "CT"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(1, created.GetProperty("requiredStaff").GetInt32());
    }

    [Fact]
    public async Task CreatedOpenShift_IsCoveredByOneEmployee_WhenScheduleIsGenerated()
    {
        await Client.PostAsJsonAsync("/api/employees", new { name = "Ida", skills = new[] { "CT" } });
        await Client.PostAsJsonAsync("/api/shifts", new
        {
            date = "2026-05-14",
            shiftType = "Day",
            requiredSkill = "CT",
            requiredStaff = 1
        });

        var generated = await (await Client.PostAsJsonAsync(
                "/api/schedule/generate",
                new { maxAssignmentsPerEmployee = 5 }))
            .Content.ReadFromJsonAsync<JsonElement>();

        var assignment = Assert.Single(generated.GetProperty("assignments").EnumerateArray());
        Assert.True(assignment.GetProperty("wasAssigned").GetBoolean());

        var overview = await GetJsonAsync("/api/schedule/overview");
        Assert.Equal(1, overview.GetProperty("assignedShifts").GetInt32());
        Assert.Equal(0, overview.GetProperty("unassignedShifts").GetInt32());
    }

    [Fact]
    public async Task LoadAnalysis_ReturnsBadRequest_WhenRequiredStaffIsNotOne()
    {
        var response = await Client.PostAsJsonAsync("/api/load-analysis", new
        {
            employeeName = "Ida",
            skills = new[] { "CT" },
            threshold = 5,
            shifts = new[]
            {
                new { date = "2026-05-11", shiftType = "Day", requiredSkill = "CT", requiredStaff = 2, assignEmployee = true }
            }
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(ShiftStaffingErrors.UnsupportedRequiredStaff, await ReadErrorAsync(response));
    }

    [Fact]
    public async Task LoadAnalysis_DefaultsRequiredStaffToOne_WhenOmitted()
    {
        var response = await Client.PostAsJsonAsync("/api/load-analysis", new
        {
            employeeName = "Ida",
            skills = new[] { "CT" },
            threshold = 5,
            shifts = new[]
            {
                new { date = "2026-05-11", shiftType = "Day", requiredSkill = "CT", assignEmployee = true }
            }
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
