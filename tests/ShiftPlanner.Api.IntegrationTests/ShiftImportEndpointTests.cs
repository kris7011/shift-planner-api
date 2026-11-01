using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using ShiftPlanner.Application.Imports;
using ShiftPlanner.Domain.Shifts;

namespace ShiftPlanner.Api.IntegrationTests;

public sealed class ShiftImportEndpointTests : ApiTestBase
{
    // The demo week is 11-17 May 2026; imports use the following week so they do not
    // collide with the seeded shifts.
    private const string ValidFile =
        "date,shiftType,requiredSkill,employee\n" +
        "2026-05-18,Day,CT,Kris\n" +
        "2026-05-18,Evening,MRI,Mette\n" +
        "2026-05-19,Night,Night,Sofie\n" +
        "2026-05-19,Day,UL,\n";

    [Fact]
    public async Task Preview_ValidFile_ReportsRowsAndSavesNoShifts()
    {
        await ResetDemoDataAsync();
        var shiftCountBefore = await CountShiftsAsync();

        var (status, preview) = await PreviewAsync(ValidFile, "week-21.csv");

        Assert.Equal(HttpStatusCode.OK, status);
        Assert.True(preview.GetProperty("canConfirm").GetBoolean());
        Assert.Equal("Pending", preview.GetProperty("status").GetString());
        Assert.Equal("week-21.csv", preview.GetProperty("fileName").GetString());
        Assert.Equal(4, preview.GetProperty("totalRows").GetInt32());
        Assert.Equal(4, preview.GetProperty("validRows").GetInt32());
        Assert.Equal(3, preview.GetProperty("assignedShiftCount").GetInt32());
        Assert.Equal(1, preview.GetProperty("openShiftCount").GetInt32());

        var openRow = preview.GetProperty("rows")[3];
        Assert.Equal(
            "SkillNotHeld",
            openRow.GetProperty("warnings")[0].GetProperty("code").GetString());

        Assert.Equal(shiftCountBefore, await CountShiftsAsync());
    }

    [Fact]
    public async Task Confirm_CreatesTheShiftsOnce()
    {
        await ResetDemoDataAsync();
        var shiftCountBefore = await CountShiftsAsync();
        var (_, preview) = await PreviewAsync(ValidFile);
        var importId = preview.GetProperty("importId").GetGuid();

        var confirmResponse = await Client.PostAsync($"/api/shift-imports/{importId}/confirm", content: null);
        var confirmed = await confirmResponse.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.OK, confirmResponse.StatusCode);
        Assert.Equal("Confirmed", confirmed.GetProperty("status").GetString());
        Assert.Equal(4, confirmed.GetProperty("createdShiftCount").GetInt32());
        Assert.Equal(shiftCountBefore + 4, await CountShiftsAsync());

        var employees = await GetJsonAsync("/api/employees");
        var krisId = employees.EnumerateArray()
            .First(employee => employee.GetProperty("name").GetString() == "Kris")
            .GetProperty("id").GetGuid();

        var shifts = await GetJsonAsync("/api/shifts");
        Assert.Contains(shifts.EnumerateArray(), shift =>
            shift.GetProperty("date").GetString() == "2026-05-18" &&
            shift.GetProperty("shiftType").GetString() == "Day" &&
            shift.GetProperty("employeeId").GetString() == krisId.ToString());

        var secondResponse = await Client.PostAsync($"/api/shift-imports/{importId}/confirm", content: null);

        Assert.Equal(HttpStatusCode.Conflict, secondResponse.StatusCode);
        Assert.Contains("already been confirmed", await ReadErrorAsync(secondResponse));
        Assert.Equal(shiftCountBefore + 4, await CountShiftsAsync());
    }

    [Fact]
    public async Task InvalidRows_AreReportedPerRow_AndTheImportCannotBeConfirmed()
    {
        await ResetDemoDataAsync();
        var shiftCountBefore = await CountShiftsAsync();

        var (status, preview) = await PreviewAsync(
            "date,shiftType,requiredSkill,employee\n" +
            "2026-05-18,Day,CT,Kris\n" +
            "2026-05-32,Day,CT,\n" +
            "2026-05-18,Brunch,CT,\n" +
            "2026-05-18,Day,CT,Bodil\n" +
            "2026-05-18,Day,MRI,Kris\n");

        Assert.Equal(HttpStatusCode.OK, status);
        Assert.False(preview.GetProperty("canConfirm").GetBoolean());
        Assert.Equal(1, preview.GetProperty("validRows").GetInt32());
        Assert.Equal(4, preview.GetProperty("invalidRows").GetInt32());

        Assert.Equal(
            ["InvalidDate", "InvalidShiftType", "UnknownEmployee", "EmployeeLacksSkill"],
            preview.GetProperty("rows").EnumerateArray().Skip(1)
                .Select(row => row.GetProperty("errors")[0].GetProperty("code").GetString()));

        Assert.Equal(
            [2, 3, 4, 5, 6],
            preview.GetProperty("rows").EnumerateArray()
                .Select(row => row.GetProperty("rowNumber").GetInt32()));

        var confirmResponse = await Client.PostAsync(
            $"/api/shift-imports/{preview.GetProperty("importId").GetGuid()}/confirm",
            content: null);

        Assert.Equal(HttpStatusCode.Conflict, confirmResponse.StatusCode);
        Assert.Equal(shiftCountBefore, await CountShiftsAsync());
    }

    [Fact]
    public async Task DuplicateRows_AssignmentsAreErrors_OpenShiftsAreWarnings()
    {
        await ResetDemoDataAsync();

        var (_, preview) = await PreviewAsync(
            "date,shiftType,requiredSkill,employee\n" +
            "2026-05-18,Day,CT,Kris\n" +
            "2026-05-18,Day,CT,kris\n" +
            "2026-05-20,Night,MRI,\n" +
            "2026-05-20,Night,MRI,\n");

        var rows = preview.GetProperty("rows");

        var duplicate = rows[1].GetProperty("errors")[0];
        Assert.Equal("DuplicateRow", duplicate.GetProperty("code").GetString());
        Assert.Equal(2, duplicate.GetProperty("relatedRowNumber").GetInt32());

        Assert.True(rows[3].GetProperty("isValid").GetBoolean());
        Assert.Equal(
            "RepeatedOpenShift",
            rows[3].GetProperty("warnings")[0].GetProperty("code").GetString());
    }

    [Fact]
    public async Task AssignmentAlreadyInTheSchedule_IsADuplicate()
    {
        await ResetDemoDataAsync();
        var seeded = (await GetJsonAsync("/api/shifts")).EnumerateArray()
            .First(shift => shift.GetProperty("employeeId").ValueKind == JsonValueKind.String);
        var employeeName = (await GetJsonAsync("/api/employees")).EnumerateArray()
            .First(employee => employee.GetProperty("id").GetString() == seeded.GetProperty("employeeId").GetString())
            .GetProperty("name").GetString();

        var (_, preview) = await PreviewAsync(
            "date,shiftType,requiredSkill,employee\n" +
            $"{seeded.GetProperty("date").GetString()},{seeded.GetProperty("shiftType").GetString()}," +
            $"{seeded.GetProperty("requiredSkill").GetString()},{employeeName}\n");

        Assert.Equal(
            "AlreadyScheduled",
            preview.GetProperty("rows")[0].GetProperty("errors")[0].GetProperty("code").GetString());
    }

    [Fact]
    public async Task SameFile_CannotBeImportedTwice_EvenWithOtherLineEndings()
    {
        await ResetDemoDataAsync();
        await ImportAsync(ValidFile);
        var shiftCount = await CountShiftsAsync();

        var (_, preview) = await PreviewAsync("﻿" + ValidFile.Replace("\n", "\r\n"));

        Assert.False(preview.GetProperty("canConfirm").GetBoolean());
        Assert.Contains(
            "already imported",
            preview.GetProperty("fileErrors")[0].GetString());

        var confirmResponse = await Client.PostAsync(
            $"/api/shift-imports/{preview.GetProperty("importId").GetGuid()}/confirm",
            content: null);

        Assert.Equal(HttpStatusCode.Conflict, confirmResponse.StatusCode);
        Assert.Equal(shiftCount, await CountShiftsAsync());
    }

    [Fact]
    public async Task Confirm_RevalidatesAgainstTheScheduleAsItIsNow()
    {
        await ResetDemoDataAsync();
        var (_, preview) = await PreviewAsync(
            "date,shiftType,requiredSkill,employee\n" +
            "2026-05-21,Day,CT,Kris\n");
        Assert.True(preview.GetProperty("canConfirm").GetBoolean());

        // After the preview, Kris gets another shift on the same day.
        var krisId = (await GetJsonAsync("/api/employees")).EnumerateArray()
            .First(employee => employee.GetProperty("name").GetString() == "Kris")
            .GetProperty("id").GetGuid();
        var createResponse = await Client.PostAsJsonAsync("/api/shifts", new
        {
            date = "2026-05-21",
            shiftType = "Evening",
            requiredSkill = "XR",
            employeeId = krisId
        });
        createResponse.EnsureSuccessStatusCode();
        var shiftCount = await CountShiftsAsync();

        var confirmResponse = await Client.PostAsync(
            $"/api/shift-imports/{preview.GetProperty("importId").GetGuid()}/confirm",
            content: null);
        var body = await confirmResponse.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.Conflict, confirmResponse.StatusCode);
        Assert.Equal(
            "Employee is already assigned to a shift on the same day.",
            body.GetProperty("preview").GetProperty("rows")[0].GetProperty("errors")[0]
                .GetProperty("message").GetString());
        Assert.Equal(shiftCount, await CountShiftsAsync());
    }

    [Fact]
    public async Task Preview_UsesTheSchedulingRulesOfScheduleGeneration()
    {
        await ResetDemoDataAsync();

        var (_, preview) = await PreviewAsync(
            "date,shiftType,requiredSkill,employee\n" +
            "2026-05-18,Night,Night,Sofie\n" +
            "2026-05-19,Day,XR,Sofie\n",
            maxAssignmentsPerEmployee: 5);

        Assert.Contains(
            preview.GetProperty("rows")[1].GetProperty("errors").EnumerateArray(),
            error => error.GetProperty("message").GetString() ==
                     "Employee cannot be assigned to a day shift directly after a night shift.");
    }

    [Theory]
    [InlineData("date,shiftType\n2026-05-18,Day\n", "The required column 'requiredSkill' is missing.")]
    [InlineData("", "The file is empty.")]
    public async Task Preview_UnreadableFile_ReturnsBadRequestAndStoresNothing(string content, string expected)
    {
        var (status, body) = await PreviewAsync(content);

        Assert.Equal(HttpStatusCode.BadRequest, status);
        Assert.Equal(expected, body.GetProperty("fileErrors")[0].GetString());
    }

    [Fact]
    public async Task Preview_RejectsAFileOverTheSizeLimit()
    {
        var content = "date,shiftType,requiredSkill,employee\n" + new string('x', 300 * 1024);

        var (status, body) = await PreviewAsync(content);

        Assert.Equal(HttpStatusCode.BadRequest, status);
        Assert.Contains("larger than", body.GetProperty("error").GetString());
    }

    [Fact]
    public async Task Confirm_UnknownImport_ReturnsNotFound()
    {
        var response = await Client.PostAsync($"/api/shift-imports/{Guid.NewGuid()}/confirm", content: null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Repository_ConfirmsAnImportOnlyOnce()
    {
        await ResetDemoDataAsync();
        var (_, preview) = await PreviewAsync(ValidFile);
        var importId = preview.GetProperty("importId").GetGuid();
        Shift shift = new(new DateOnly(2026, 5, 22), ShiftType.Day, "CT", 1);
        var shiftCount = await CountShiftsAsync();

        // Two confirmations that both passed validation: only the first may write.
        using (var scope = Services.CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IShiftImportRepository>();
            Assert.Equal(
                ShiftImportConfirmationResult.Confirmed,
                await repository.ConfirmAsync(importId, [shift], DateTimeOffset.UtcNow));
        }

        using (var scope = Services.CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IShiftImportRepository>();
            Assert.Equal(
                ShiftImportConfirmationResult.AlreadyConfirmed,
                await repository.ConfirmAsync(importId, [new Shift(shift.Date, shift.ShiftType, "CT", 1)], DateTimeOffset.UtcNow));
        }

        Assert.Equal(shiftCount + 1, await CountShiftsAsync());
    }

    [Fact]
    public async Task Repository_RefusesASecondConfirmedImportOfTheSameContent()
    {
        await ResetDemoDataAsync();
        var (_, first) = await PreviewAsync(ValidFile);
        var (_, second) = await PreviewAsync(ValidFile);
        var shiftCount = await CountShiftsAsync();

        // Both previews were taken before either was confirmed, so the service's own check
        // passed for both; the unique index is what stops the second.
        using var scope = Services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IShiftImportRepository>();

        Assert.Equal(
            ShiftImportConfirmationResult.Confirmed,
            await repository.ConfirmAsync(
                first.GetProperty("importId").GetGuid(),
                [new Shift(new DateOnly(2026, 5, 22), ShiftType.Day, "CT", 1)],
                DateTimeOffset.UtcNow));

        Assert.Equal(
            ShiftImportConfirmationResult.DuplicateContent,
            await repository.ConfirmAsync(
                second.GetProperty("importId").GetGuid(),
                [new Shift(new DateOnly(2026, 5, 23), ShiftType.Day, "CT", 1)],
                DateTimeOffset.UtcNow));

        Assert.Equal(shiftCount + 1, await CountShiftsAsync());

        var secondImport = await repository.GetAsync(second.GetProperty("importId").GetGuid());
        Assert.Equal(ShiftImportStatus.Pending, secondImport!.Status);
    }

    [Fact]
    public async Task DemoReset_ClearsImportHistory_SoTheFileCanBeImportedAgain()
    {
        await ResetDemoDataAsync();
        await ImportAsync(ValidFile);

        await ResetDemoDataAsync();
        var (_, preview) = await PreviewAsync(ValidFile);

        Assert.True(preview.GetProperty("canConfirm").GetBoolean());
    }

    [Fact]
    public async Task Export_ReturnsTheWeekInTheImportFormat_AndIncludesImportedShifts()
    {
        await ResetDemoDataAsync();
        await ImportAsync(ValidFile);

        var response = await Client.GetAsync("/api/schedule/export?weekStart=2026-05-20");
        var bytes = await response.Content.ReadAsByteArrayAsync();
        var content = Encoding.UTF8.GetString(bytes).TrimStart('﻿');

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/csv", response.Content.Headers.ContentType?.MediaType);
        Assert.Equal("shift-schedule-2026-05-18.csv", response.Content.Headers.ContentDisposition?.FileNameStar);
        Assert.Equal([0xEF, 0xBB, 0xBF], bytes.Take(3));
        Assert.Equal(
            "date,shiftType,requiredSkill,employee\r\n" +
            "2026-05-18,Day,CT,Kris\r\n" +
            "2026-05-18,Evening,MRI,Mette\r\n" +
            "2026-05-19,Day,UL,\r\n" +
            "2026-05-19,Night,Night,Sofie\r\n",
            content);
    }

    [Fact]
    public async Task Export_OfTheDemoWeek_CanBeImportedIntoAFreshSchedule()
    {
        await ResetDemoDataAsync();
        var exported = await Client.GetStringAsync("/api/schedule/export?weekStart=2026-05-11");
        var exportedRows = exported.Split("\r\n", StringSplitOptions.RemoveEmptyEntries).Length - 1;

        // Same employees, no shifts: the exported week must import without errors.
        using (var scope = Services.CreateScope())
        {
            await scope.ServiceProvider.GetRequiredService<Application.Shifts.IShiftRepository>().DeleteAllAsync();
        }

        var (_, preview) = await PreviewAsync(exported);

        Assert.Equal(14, exportedRows);
        Assert.True(preview.GetProperty("canConfirm").GetBoolean(), preview.ToString());
        Assert.Equal(exportedRows, preview.GetProperty("validRows").GetInt32());
    }

    private async Task<(HttpStatusCode Status, JsonElement Body)> PreviewAsync(
        string csv,
        string fileName = "shifts.csv",
        int? maxAssignmentsPerEmployee = null)
    {
        var query = $"?fileName={Uri.EscapeDataString(fileName)}";

        if (maxAssignmentsPerEmployee is not null)
        {
            query += $"&maxAssignmentsPerEmployee={maxAssignmentsPerEmployee}";
        }

        var response = await Client.PostAsync(
            "/api/shift-imports/preview" + query,
            new StringContent(csv, Encoding.UTF8, "text/csv"));

        return (response.StatusCode, await response.Content.ReadFromJsonAsync<JsonElement>());
    }

    private async Task ImportAsync(string csv)
    {
        var (_, preview) = await PreviewAsync(csv);
        var response = await Client.PostAsync(
            $"/api/shift-imports/{preview.GetProperty("importId").GetGuid()}/confirm",
            content: null);

        response.EnsureSuccessStatusCode();
    }

    private async Task<int> CountShiftsAsync()
    {
        return (await GetJsonAsync("/api/shifts")).GetArrayLength();
    }
}
