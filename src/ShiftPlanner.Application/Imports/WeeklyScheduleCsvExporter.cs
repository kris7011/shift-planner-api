using System.Globalization;
using System.Text;
using ShiftPlanner.Domain.Employees;
using ShiftPlanner.Domain.Shifts;

namespace ShiftPlanner.Application.Imports;

public class WeeklyScheduleExport
{
    public DateOnly WeekStart { get; set; }

    public DateOnly WeekEnd { get; set; }

    public string FileName { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public int ShiftCount { get; set; }
}

/// <summary>
/// Writes one Monday-to-Sunday week of the schedule as CSV in the import format,
/// so the file can be edited in a spreadsheet and imported again.
/// </summary>
public class WeeklyScheduleCsvExporter
{
    public static DateOnly StartOfWeek(DateOnly date)
    {
        var daysSinceMonday = ((int)date.DayOfWeek + 6) % 7;

        return date.AddDays(-daysSinceMonday);
    }

    public WeeklyScheduleExport Export(
        DateOnly anyDateInWeek,
        List<Shift> shifts,
        List<Employee> employees)
    {
        var weekStart = StartOfWeek(anyDateInWeek);
        var weekEnd = weekStart.AddDays(6);
        var employeeNames = employees.ToDictionary(employee => employee.Id, employee => employee.Name);

        var weekShifts = shifts
            .Where(shift => shift.Date >= weekStart && shift.Date <= weekEnd)
            .OrderBy(shift => shift.Date)
            .ThenBy(shift => shift.ShiftType)
            .ThenBy(shift => shift.RequiredSkill, StringComparer.Ordinal)
            .ThenBy(shift => EmployeeName(shift, employeeNames), StringComparer.Ordinal)
            .ToList();

        var content = new StringBuilder();
        content.Append(ShiftCsvFormat.FormatLine(ShiftCsvFormat.Columns)).Append("\r\n");

        foreach (var shift in weekShifts)
        {
            content.Append(ShiftCsvFormat.FormatLine(
                [
                    shift.Date.ToString(ShiftCsvFormat.DateFormat, CultureInfo.InvariantCulture),
                    shift.ShiftType.ToString(),
                    shift.RequiredSkill,
                    EmployeeName(shift, employeeNames)
                ]))
                .Append("\r\n");
        }

        return new WeeklyScheduleExport
        {
            WeekStart = weekStart,
            WeekEnd = weekEnd,
            FileName = $"shift-schedule-{weekStart.ToString(ShiftCsvFormat.DateFormat, CultureInfo.InvariantCulture)}.csv",
            Content = content.ToString(),
            ShiftCount = weekShifts.Count
        };
    }

    private static string EmployeeName(Shift shift, Dictionary<Guid, string> employeeNames)
    {
        if (shift.EmployeeId is null)
        {
            return string.Empty;
        }

        // A shift whose employee no longer exists keeps its id so the export does not lose it.
        return employeeNames.TryGetValue(shift.EmployeeId.Value, out var name)
            ? name
            : shift.EmployeeId.Value.ToString();
    }
}
