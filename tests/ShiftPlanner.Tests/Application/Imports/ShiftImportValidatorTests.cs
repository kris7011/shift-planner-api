using ShiftPlanner.Application.Imports;
using ShiftPlanner.Domain.Employees;
using ShiftPlanner.Domain.Shifts;
using ShiftPlanner.Tests.Helpers;

namespace ShiftPlanner.Tests.Application.Imports;

public class ShiftImportValidatorTests
{
    private const string Header = "date,shiftType,requiredSkill,employee\n";

    private readonly Employee _kris = new("Kris", ["CT", "XR"]);
    private readonly Employee _mette = new("Mette", ["MRI", "Night"]);

    private readonly ShiftImportValidator _validator = new(SchedulingRulesFactory.Create());

    [Fact]
    public void Validate_ValidRows_CreateOpenAndAssignedShifts()
    {
        var validation = Validate(
            "2026-05-18,day,ct,kris\n" +
            "2026-05-18,Night,MRI,\n");

        Assert.All(validation.Rows, row => Assert.True(row.IsValid));
        Assert.Equal(2, validation.Shifts.Count);

        var assigned = validation.Shifts[0];
        Assert.Equal(new DateOnly(2026, 5, 18), assigned.Date);
        Assert.Equal(ShiftType.Day, assigned.ShiftType);
        Assert.Equal("CT", assigned.RequiredSkill);
        Assert.Equal(_kris.Id, assigned.EmployeeId);
        Assert.Equal("Kris", validation.Rows[0].EmployeeName);

        Assert.Null(validation.Shifts[1].EmployeeId);
    }

    [Theory]
    [InlineData("18-05-2026,Day,CT,", ShiftImportIssueCodes.InvalidDate)]
    [InlineData("2026-02-30,Day,CT,", ShiftImportIssueCodes.InvalidDate)]
    [InlineData("2026-05-18,Morning,CT,", ShiftImportIssueCodes.InvalidShiftType)]
    [InlineData("2026-05-18,2,CT,", ShiftImportIssueCodes.InvalidShiftType)]
    [InlineData("2026-05-18,Day,,", ShiftImportIssueCodes.MissingRequiredSkill)]
    [InlineData("2026-05-18,Day,=HYPERLINK(1),", ShiftImportIssueCodes.UnsafeValue)]
    [InlineData("2026-05-18,Day,CT,Bodil", ShiftImportIssueCodes.UnknownEmployee)]
    [InlineData("2026-05-18,Day,MRI,Kris", ShiftImportIssueCodes.EmployeeLacksSkill)]
    [InlineData("2026-05-18,Day,CT,Kris,extra", ShiftImportIssueCodes.FieldCount)]
    public void Validate_InvalidRow_IsReportedAndNotImported(string row, string expectedCode)
    {
        var validation = Validate(row + "\n2026-05-19,Day,CT,\n");

        var invalid = validation.Rows[0];
        Assert.False(invalid.IsValid);
        Assert.Contains(invalid.Errors, error => error.Code == expectedCode);

        // Other rows are still checked and reported on their own.
        Assert.True(validation.Rows[1].IsValid);
        Assert.Single(validation.Shifts);
    }

    [Fact]
    public void Validate_ReportsEveryProblemOfARow()
    {
        var validation = Validate("tomorrow,Morning,,\n");

        var codes = validation.Rows[0].Errors.Select(error => error.Code).ToList();

        Assert.Equal(
            [
                ShiftImportIssueCodes.InvalidDate,
                ShiftImportIssueCodes.InvalidShiftType,
                ShiftImportIssueCodes.MissingRequiredSkill
            ],
            codes);
    }

    [Fact]
    public void Validate_AmbiguousEmployeeName_IsAnError()
    {
        var secondKris = new Employee("Kris", ["CT"]);

        var validation = Validate("2026-05-18,Day,CT,Kris\n", employees: [_kris, secondKris]);

        Assert.Equal(
            ShiftImportIssueCodes.AmbiguousEmployee,
            Assert.Single(validation.Rows[0].Errors).Code);
    }

    [Fact]
    public void Validate_DuplicateAssignmentInFile_PointsToTheFirstRow()
    {
        var validation = Validate(
            "2026-05-18,Day,CT,Kris\n" +
            "2026-05-18,Day,CT,Kris\n");

        Assert.True(validation.Rows[0].IsValid);

        var duplicate = Assert.Single(validation.Rows[1].Errors);
        Assert.Equal(ShiftImportIssueCodes.DuplicateRow, duplicate.Code);
        Assert.Equal(2, duplicate.RelatedRowNumber);
        Assert.Single(validation.Shifts);
    }

    [Fact]
    public void Validate_AssignmentAlreadyInSchedule_IsAnError()
    {
        var existing = new Shift(new DateOnly(2026, 5, 18), ShiftType.Day, "CT", 1, _kris.Id);

        var validation = Validate("2026-05-18,Day,CT,Kris\n", existingShifts: [existing]);

        Assert.Equal(
            ShiftImportIssueCodes.AlreadyScheduled,
            Assert.Single(validation.Rows[0].Errors).Code);
    }

    [Fact]
    public void Validate_RepeatedOpenShifts_AreAllowedWithAWarning()
    {
        var existingOpen = new Shift(new DateOnly(2026, 5, 18), ShiftType.Night, "MRI", 1);

        var validation = Validate(
            "2026-05-18,Night,MRI,\n" +
            "2026-05-18,Night,MRI,\n",
            existingShifts: [existingOpen]);

        Assert.All(validation.Rows, row => Assert.True(row.IsValid));
        Assert.Equal(2, validation.Shifts.Count);

        Assert.Equal(
            [ShiftImportIssueCodes.OpenShiftExists],
            validation.Rows[0].Warnings.Select(warning => warning.Code));

        Assert.Contains(validation.Rows[1].Warnings, warning =>
            warning.Code == ShiftImportIssueCodes.RepeatedOpenShift && warning.RelatedRowNumber == 2);
    }

    [Fact]
    public void Validate_SkillNobodyHas_IsAWarningForAnOpenShift()
    {
        var validation = Validate("2026-05-18,Day,UL,\n");

        Assert.True(validation.Rows[0].IsValid);
        Assert.Equal(
            ShiftImportIssueCodes.SkillNotHeld,
            Assert.Single(validation.Rows[0].Warnings).Code);
    }

    [Fact]
    public void Validate_AppliesTheSameDayRuleAgainstTheExistingSchedule()
    {
        var existing = new Shift(new DateOnly(2026, 5, 18), ShiftType.Evening, "XR", 1, _kris.Id);

        var validation = Validate("2026-05-18,Day,CT,Kris\n", existingShifts: [existing]);

        var error = Assert.Single(validation.Rows[0].Errors);
        Assert.Equal(ShiftImportIssueCodes.SchedulingRule, error.Code);
        Assert.Equal("Employee is already assigned to a shift on the same day.", error.Message);
    }

    [Fact]
    public void Validate_AppliesTheNightToDayRuleBetweenRowsOfTheFile()
    {
        var validation = Validate(
            "2026-05-18,Night,MRI,Mette\n" +
            "2026-05-19,Day,MRI,Mette\n");

        Assert.True(validation.Rows[0].IsValid);
        Assert.Contains(validation.Rows[1].Errors, error =>
            error.Message == "Employee cannot be assigned to a day shift directly after a night shift.");
    }

    [Fact]
    public void Validate_AppliesTheMaximumNumberOfAssignments()
    {
        var validation = Validate(
            "2026-05-18,Day,CT,Kris\n" +
            "2026-05-19,Day,CT,Kris\n",
            maxAssignmentsPerEmployee: 1);

        Assert.True(validation.Rows[0].IsValid);
        Assert.Contains(validation.Rows[1].Errors, error =>
            error.Message == "Employee has reached the maximum number of assignments.");
    }

    [Fact]
    public void Validate_RejectedRowsDoNotCountAsPlanned()
    {
        // Row 2 is rejected, so it must not block Kris's shift the day after a night shift.
        var validation = Validate(
            "2026-05-18,Night,MRI,Kris\n" +
            "2026-05-19,Day,CT,Kris\n");

        Assert.False(validation.Rows[0].IsValid);
        Assert.True(validation.Rows[1].IsValid);
    }

    private ShiftImportValidation Validate(
        string rows,
        List<Employee>? employees = null,
        List<Shift>? existingShifts = null,
        int maxAssignmentsPerEmployee = 5)
    {
        return _validator.Validate(
            ShiftCsvParser.Parse(Header + rows),
            employees ?? [_kris, _mette],
            existingShifts ?? [],
            maxAssignmentsPerEmployee);
    }
}
