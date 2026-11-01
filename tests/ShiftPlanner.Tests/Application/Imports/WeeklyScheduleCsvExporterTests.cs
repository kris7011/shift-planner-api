using ShiftPlanner.Application.Imports;
using ShiftPlanner.Domain.Employees;
using ShiftPlanner.Domain.Shifts;
using ShiftPlanner.Tests.Helpers;

namespace ShiftPlanner.Tests.Application.Imports;

public class WeeklyScheduleCsvExporterTests
{
    private readonly Employee _kris = new("Kris", ["CT"]);
    private readonly Employee _mette = new("Mette", ["MRI"]);
    private readonly WeeklyScheduleCsvExporter _exporter = new();

    [Theory]
    [InlineData(2026, 5, 11)]
    [InlineData(2026, 5, 13)]
    [InlineData(2026, 5, 17)]
    public void StartOfWeek_ReturnsTheMonday(int year, int month, int day)
    {
        Assert.Equal(
            new DateOnly(2026, 5, 11),
            WeeklyScheduleCsvExporter.StartOfWeek(new DateOnly(year, month, day)));
    }

    [Fact]
    public void Export_WritesTheWeekSortedInTheImportFormat()
    {
        List<Shift> shifts =
        [
            new(new DateOnly(2026, 5, 12), ShiftType.Night, "MRI", 1),
            new(new DateOnly(2026, 5, 11), ShiftType.Evening, "MRI", 1, _mette.Id),
            new(new DateOnly(2026, 5, 11), ShiftType.Day, "CT", 1, _kris.Id),
            new(new DateOnly(2026, 5, 18), ShiftType.Day, "CT", 1, _kris.Id),
            new(new DateOnly(2026, 5, 10), ShiftType.Day, "CT", 1, _kris.Id)
        ];

        var export = _exporter.Export(new DateOnly(2026, 5, 14), shifts, [_kris, _mette]);

        Assert.Equal(new DateOnly(2026, 5, 11), export.WeekStart);
        Assert.Equal(new DateOnly(2026, 5, 17), export.WeekEnd);
        Assert.Equal("shift-schedule-2026-05-11.csv", export.FileName);
        Assert.Equal(3, export.ShiftCount);
        Assert.Equal(
            "date,shiftType,requiredSkill,employee\r\n" +
            "2026-05-11,Day,CT,Kris\r\n" +
            "2026-05-11,Evening,MRI,Mette\r\n" +
            "2026-05-12,Night,MRI,\r\n",
            export.Content);
    }

    [Fact]
    public void Export_QuotesDelimitersAndNeutralisesFormulas()
    {
        var employee = new Employee("=cmd|' /C calc'!A0", ["CT, contrast"]);
        Shift shift = new(new DateOnly(2026, 5, 11), ShiftType.Day, "CT, contrast", 1, employee.Id);

        var export = _exporter.Export(new DateOnly(2026, 5, 11), [shift], [employee]);

        Assert.EndsWith("2026-05-11,Day,\"CT, contrast\",'=cmd|' /C calc'!A0\r\n", export.Content);
    }

    [Fact]
    public void Export_CanBeImportedAgain()
    {
        List<Shift> shifts =
        [
            new(new DateOnly(2026, 5, 11), ShiftType.Day, "CT", 1, _kris.Id),
            new(new DateOnly(2026, 5, 11), ShiftType.Night, "MRI", 1),
            new(new DateOnly(2026, 5, 13), ShiftType.Evening, "MRI", 1, _mette.Id)
        ];

        var export = _exporter.Export(new DateOnly(2026, 5, 11), shifts, [_kris, _mette]);

        var validation = new ShiftImportValidator(SchedulingRulesFactory.Create()).Validate(
            ShiftCsvParser.Parse(export.Content),
            [_kris, _mette],
            existingShifts: [],
            maxAssignmentsPerEmployee: 5);

        Assert.All(validation.Rows, row => Assert.True(row.IsValid));
        Assert.Equal(
            shifts.Select(shift => (shift.Date, shift.ShiftType, shift.RequiredSkill, shift.EmployeeId)),
            validation.Shifts.Select(shift => (shift.Date, shift.ShiftType, shift.RequiredSkill, shift.EmployeeId)));
    }
}
