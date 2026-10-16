using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace ShiftPlanner.Api.IntegrationTests;

public sealed class EmployeeAndShiftEndpointTests : ApiTestBase
{
    [Fact]
    public async Task CreateEmployee_ReturnsCreated_AndEmployeeIsListed()
    {
        var response = await Client.PostAsJsonAsync("/api/employees", new
        {
            name = "Ida",
            skills = new[] { "CT", "MRI" }
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<JsonElement>();
        var id = created.GetProperty("id").GetGuid();
        Assert.Equal($"/api/employees/{id}", response.Headers.Location?.OriginalString);

        var employees = await GetJsonAsync("/api/employees");
        var employee = Assert.Single(employees.EnumerateArray());
        Assert.Equal("Ida", employee.GetProperty("name").GetString());
        Assert.Equal(["CT", "MRI"], employee.GetProperty("skills").EnumerateArray().Select(skill => skill.GetString()));
    }

    [Theory]
    [InlineData("", "CT", "Employee name is required.")]
    [InlineData("Ida", null, "At least one skill is required.")]
    public async Task CreateEmployee_ReturnsBadRequestWithError_WhenRequestIsInvalid(
        string name,
        string? skill,
        string expectedError)
    {
        var response = await Client.PostAsJsonAsync("/api/employees", new
        {
            name,
            skills = skill is null ? Array.Empty<string>() : new[] { skill }
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(expectedError, await ReadErrorAsync(response));
    }

    [Fact]
    public async Task EmployeeLoadDetails_ReturnsNotFound_ForUnknownEmployee()
    {
        var response = await Client.GetAsync($"/api/employees/{Guid.NewGuid()}/load-details");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("Employee was not found.", await ReadErrorAsync(response));
    }

    [Fact]
    public async Task LoadOverviewAndPreferenceProfiles_CoverAllDemoEmployees()
    {
        await ResetDemoDataAsync();

        var loadOverview = await GetJsonAsync("/api/employees/load-overview");
        var profiles = await GetJsonAsync("/api/employees/preference-profiles");

        Assert.Equal(10, loadOverview.GetArrayLength());
        Assert.Equal(10, profiles.GetArrayLength());

        var henrik = profiles.EnumerateArray()
            .Single(profile => profile.GetProperty("employeeName").GetString() == "Henrik");
        Assert.Equal("Night", henrik.GetProperty("preferredShiftTypes")[0].GetString());
    }

    [Fact]
    public async Task AssignmentAnalysis_ExplainsWhyUnassignedShiftCannotBeCovered()
    {
        await ResetDemoDataAsync();
        var shifts = await GetJsonAsync("/api/shifts");
        var ultrasoundShiftId = shifts.EnumerateArray()
            .Single(shift => shift.GetProperty("requiredSkill").GetString() == "UL")
            .GetProperty("id")
            .GetGuid();

        var analysis = await GetJsonAsync($"/api/shifts/{ultrasoundShiftId}/assignment-analysis?maxAssignmentsPerEmployee=5");

        Assert.False(analysis.GetProperty("canBeCovered").GetBoolean());
        var candidates = analysis.GetProperty("candidateResults").EnumerateArray().ToList();
        Assert.Equal(10, candidates.Count);
        Assert.All(candidates, candidate => Assert.False(candidate.GetProperty("canBeAssigned").GetBoolean()));
    }

    [Fact]
    public async Task AssignmentAnalysis_ReturnsNotFound_ForUnknownShift()
    {
        var response = await Client.GetAsync($"/api/shifts/{Guid.NewGuid()}/assignment-analysis");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("Shift was not found.", await ReadErrorAsync(response));
    }

    [Fact]
    public async Task LoadAnalysis_ReturnsBadRequest_WhenEmployeeLacksSkillForAssignedShift()
    {
        // Previously this threw InvalidOperationException and returned 500.
        var response = await Client.PostAsJsonAsync("/api/load-analysis", new
        {
            employeeName = "Ida",
            skills = new[] { "CT" },
            threshold = 5,
            shifts = new[]
            {
                new { date = "2026-05-11", shiftType = "Day", requiredSkill = "MRI", requiredStaff = 1, assignEmployee = true }
            }
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(
            "Employee does not have the required skill 'MRI' for the shift on 2026-05-11.",
            await ReadErrorAsync(response));
    }

    [Fact]
    public async Task LoadAnalysis_CalculatesLoadForAssignedShifts()
    {
        var response = await Client.PostAsJsonAsync("/api/load-analysis", new
        {
            employeeName = "Ida",
            skills = new[] { "CT", "Night" },
            threshold = 5,
            shifts = new[]
            {
                new { date = "2026-05-11", shiftType = "Day", requiredSkill = "CT", requiredStaff = 1, assignEmployee = true },
                new { date = "2026-05-12", shiftType = "Night", requiredSkill = "Night", requiredStaff = 1, assignEmployee = true },
                new { date = "2026-05-13", shiftType = "Day", requiredSkill = "CT", requiredStaff = 1, assignEmployee = false }
            }
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Ida", result.GetProperty("employeeName").GetString());
        Assert.True(result.GetProperty("totalLoad").GetInt32() > 0);
    }
}
