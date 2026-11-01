using System.Text;
using ShiftPlanner.Application.Employees;
using ShiftPlanner.Application.Imports;
using ShiftPlanner.Application.Shifts;

namespace ShiftPlanner.Api.Imports;

public static class ShiftImportEndpoints
{
    public const int MaxFileCharacters = 256 * 1024;
    public const int DefaultMaxAssignmentsPerEmployee = 5;

    public static void MapShiftImportEndpoints(this WebApplication app)
    {
        app.MapPost("/api/shift-imports/preview", async (
            HttpRequest request,
            string? fileName,
            int? maxAssignmentsPerEmployee,
            IShiftImportService shiftImportService) =>
        {
            var maxAssignments = maxAssignmentsPerEmployee ?? DefaultMaxAssignmentsPerEmployee;

            if (maxAssignments <= 0)
            {
                return Results.BadRequest(new
                {
                    error = "MaxAssignmentsPerEmployee must be greater than 0."
                });
            }

            var content = await ReadBodyAsync(request);

            if (content is null)
            {
                return Results.BadRequest(new
                {
                    error = $"The file is larger than {MaxFileCharacters / 1024} KB."
                });
            }

            var (preview, fileErrors) = await shiftImportService.PreviewAsync(
                SafeFileName(fileName),
                content,
                maxAssignments);

            if (preview is null)
            {
                return Results.BadRequest(new
                {
                    error = "The file could not be read as a shift import.",
                    fileErrors
                });
            }

            return Results.Ok(preview);
        })
        .Accepts<string>("text/csv")
        .WithName("PreviewShiftImport")
        .WithSummary("Validates a CSV of shifts without saving them")
        .WithDescription("Reads the CSV request body (columns date, shiftType, requiredSkill, employee), validates every row against the existing schedule and scheduling rules, and stores it as a pending import. No shift is created until the import is confirmed.")
        .Produces<ShiftImportPreviewResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest);

        app.MapPost("/api/shift-imports/{id:guid}/confirm", async (
            Guid id,
            IShiftImportService shiftImportService) =>
        {
            var result = await shiftImportService.ConfirmAsync(id);

            return result.Outcome switch
            {
                ConfirmShiftImportOutcome.NotFound => Results.NotFound(new
                {
                    error = result.Error
                }),
                ConfirmShiftImportOutcome.AlreadyConfirmed => Results.Conflict(new
                {
                    error = result.Error,
                    result.ImportId,
                    result.CreatedShiftCount,
                    result.ConfirmedAt
                }),
                ConfirmShiftImportOutcome.NoLongerValid => Results.Conflict(new
                {
                    error = result.Error,
                    preview = result.Preview
                }),
                _ => Results.Ok(new
                {
                    result.ImportId,
                    Status = ShiftImportStatus.Confirmed,
                    result.CreatedShiftCount,
                    result.ConfirmedAt
                })
            };
        })
        .WithName("ConfirmShiftImport")
        .WithSummary("Saves the shifts of a previewed import")
        .WithDescription("Validates the stored file again against the current schedule and, when every row is still valid, creates all its shifts in one transaction. An import can be confirmed once.")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status409Conflict);

        app.MapGet("/api/schedule/export", async (
            DateOnly weekStart,
            IShiftRepository shiftRepository,
            IEmployeeRepository employeeRepository,
            WeeklyScheduleCsvExporter exporter) =>
        {
            var shifts = await shiftRepository.GetAllAsync();
            var employees = await employeeRepository.GetAllAsync();

            var export = exporter.Export(weekStart, shifts, employees);

            // The byte order mark lets spreadsheet programs read Danish letters correctly;
            // the import ignores it.
            var bytes = Encoding.UTF8.GetPreamble()
                .Concat(Encoding.UTF8.GetBytes(export.Content))
                .ToArray();

            return Results.File(bytes, "text/csv; charset=utf-8", export.FileName);
        })
        .WithName("ExportWeeklySchedule")
        .WithSummary("Exports one week of the schedule as CSV")
        .WithDescription("Returns the Monday-to-Sunday week that contains weekStart in the import format, so it can be edited and imported again.")
        .Produces(StatusCodes.Status200OK, contentType: "text/csv");
    }

    private static async Task<string?> ReadBodyAsync(HttpRequest request)
    {
        using var reader = new StreamReader(request.Body, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);

        var buffer = new char[MaxFileCharacters + 1];
        var length = await reader.ReadBlockAsync(buffer, 0, buffer.Length);

        return length > MaxFileCharacters ? null : new string(buffer, 0, length);
    }

    private static string SafeFileName(string? fileName)
    {
        var name = Path.GetFileName(fileName ?? string.Empty).Trim();

        if (name.Length == 0)
        {
            return "import.csv";
        }

        return name.Length > 255 ? name[..255] : name;
    }
}
