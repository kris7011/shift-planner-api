using System.Globalization;
using ShiftPlanner.Application.Scheduling.Rules;
using ShiftPlanner.Domain.Employees;
using ShiftPlanner.Domain.Shifts;

namespace ShiftPlanner.Application.Imports;

public class ShiftImportValidation
{
    public List<ShiftImportRowResult> Rows { get; set; } = [];

    /// <summary>The shifts the valid rows describe, in file order.</summary>
    public List<Shift> Shifts { get; set; } = [];
}

/// <summary>
/// Checks every imported row and reuses the scheduling rules that schedule generation applies,
/// so an imported assignment is held to the same standard as a generated one.
/// Rows are checked in file order: an assignment accepted earlier in the file counts as planned
/// for the rows after it, exactly like the generator's planned shifts.
/// </summary>
public class ShiftImportValidator
{
    public const int MaxSkillLength = 50;
    public const int MaxEmployeeNameLength = 100;

    private readonly IEnumerable<ISchedulingRule> _rules;

    public ShiftImportValidator(IEnumerable<ISchedulingRule> rules)
    {
        _rules = rules;
    }

    public ShiftImportValidation Validate(
        ShiftCsvParseResult parsed,
        List<Employee> employees,
        List<Shift> existingShifts,
        int maxAssignmentsPerEmployee)
    {
        var validation = new ShiftImportValidation();
        var knownSkills = employees
            .SelectMany(employee => employee.Skills)
            .Distinct(StringComparer.Ordinal)
            .ToList();

        // Persisted assignments plus the assignments accepted from earlier rows.
        var plannedShifts = existingShifts
            .Where(shift => shift.EmployeeId != null)
            .ToList();

        var acceptedRows = new List<(ShiftImportRowResult Row, Shift Shift)>();

        foreach (var csvRow in parsed.Rows)
        {
            var row = new ShiftImportRowResult
            {
                RowNumber = csvRow.RowNumber,
                Date = csvRow.Date,
                ShiftType = csvRow.ShiftType,
                RequiredSkill = csvRow.RequiredSkill,
                EmployeeName = csvRow.Employee
            };

            validation.Rows.Add(row);

            if (csvRow.FieldCount != parsed.ExpectedFieldCount)
            {
                AddError(row, ShiftImportIssueCodes.FieldCount,
                    $"Expected {parsed.ExpectedFieldCount} values but found {csvRow.FieldCount}. Check for a missing or extra delimiter.");
                continue;
            }

            var shift = ReadShift(csvRow, row, employees, knownSkills);

            if (shift is null)
            {
                continue;
            }

            if (shift.EmployeeId is null)
            {
                AddOpenShiftWarnings(row, shift, acceptedRows, existingShifts);
            }
            else
            {
                CheckAssignment(row, shift, employees, acceptedRows, existingShifts, plannedShifts, maxAssignmentsPerEmployee);
            }

            if (row.IsValid)
            {
                acceptedRows.Add((row, shift));
                validation.Shifts.Add(shift);

                if (shift.EmployeeId is not null)
                {
                    plannedShifts.Add(shift);
                }
            }
        }

        return validation;
    }

    private static Shift? ReadShift(
        ShiftCsvRow csvRow,
        ShiftImportRowResult row,
        List<Employee> employees,
        List<string> knownSkills)
    {
        foreach (var value in new[] { csvRow.RequiredSkill, csvRow.Employee })
        {
            if (ShiftCsvFormat.StartsLikeFormula(value))
            {
                AddError(row, ShiftImportIssueCodes.UnsafeValue,
                    $"'{value}' starts with a character that spreadsheet programs treat as a formula.");
            }
        }

        var hasDate = DateOnly.TryParseExact(
            csvRow.Date,
            ShiftCsvFormat.DateFormat,
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out var date);

        if (!hasDate)
        {
            AddError(row, ShiftImportIssueCodes.InvalidDate,
                $"'{csvRow.Date}' is not a date in the format {ShiftCsvFormat.DateFormat}.");
        }

        // Numbers are refused even though Enum.TryParse accepts them, so "7" is not a shift type.
        var hasShiftType =
            Enum.TryParse<ShiftType>(csvRow.ShiftType, ignoreCase: true, out var shiftType) &&
            !int.TryParse(csvRow.ShiftType, out _) &&
            Enum.IsDefined(shiftType);

        if (!hasShiftType)
        {
            AddError(row, ShiftImportIssueCodes.InvalidShiftType,
                $"'{csvRow.ShiftType}' is not a shift type. Use {string.Join(", ", Enum.GetNames<ShiftType>())}.");
        }

        var skill = csvRow.RequiredSkill;

        if (skill.Length == 0)
        {
            AddError(row, ShiftImportIssueCodes.MissingRequiredSkill, "The required skill is missing.");
        }
        else if (skill.Length > MaxSkillLength)
        {
            AddError(row, ShiftImportIssueCodes.ValueTooLong,
                $"The required skill can be at most {MaxSkillLength} characters.");
        }
        else
        {
            // Use the spelling the employees have, so "ct" matches the skill "CT".
            skill = knownSkills.FirstOrDefault(known =>
                string.Equals(known, skill, StringComparison.OrdinalIgnoreCase)) ?? skill;

            row.RequiredSkill = skill;
        }

        var employee = ResolveEmployee(csvRow.Employee, row, employees);

        if (!row.IsValid)
        {
            return null;
        }

        if (employee is null && !knownSkills.Contains(skill, StringComparer.Ordinal))
        {
            AddWarning(row, ShiftImportIssueCodes.SkillNotHeld,
                $"No employee has the skill '{skill}', so the shift will stay open.");
        }

        if (employee is not null && !employee.HasSkill(skill))
        {
            AddError(row, ShiftImportIssueCodes.EmployeeLacksSkill,
                $"{employee.Name} does not have the required skill '{skill}'.");
            return null;
        }

        return new Shift(
            date,
            shiftType,
            skill,
            Shift.SupportedRequiredStaff,
            employee?.Id);
    }

    private static Employee? ResolveEmployee(
        string name,
        ShiftImportRowResult row,
        List<Employee> employees)
    {
        if (name.Length == 0)
        {
            return null;
        }

        if (name.Length > MaxEmployeeNameLength)
        {
            AddError(row, ShiftImportIssueCodes.ValueTooLong,
                $"The employee name can be at most {MaxEmployeeNameLength} characters.");
            return null;
        }

        var matches = employees
            .Where(employee => string.Equals(employee.Name.Trim(), name, StringComparison.OrdinalIgnoreCase))
            .ToList();

        switch (matches.Count)
        {
            case 0:
                AddError(row, ShiftImportIssueCodes.UnknownEmployee,
                    $"No employee is named '{name}'. Leave the employee empty to import an open shift.");
                return null;

            case > 1:
                AddError(row, ShiftImportIssueCodes.AmbiguousEmployee,
                    $"{matches.Count} employees are named '{name}', so the row cannot be matched to one of them.");
                return null;

            default:
                row.EmployeeName = matches[0].Name;
                row.EmployeeId = matches[0].Id;
                return matches[0];
        }
    }

    private void CheckAssignment(
        ShiftImportRowResult row,
        Shift shift,
        List<Employee> employees,
        List<(ShiftImportRowResult Row, Shift Shift)> acceptedRows,
        List<Shift> existingShifts,
        List<Shift> plannedShifts,
        int maxAssignmentsPerEmployee)
    {
        var duplicate = acceptedRows.FirstOrDefault(accepted => IsSameShift(accepted.Shift, shift));

        if (duplicate.Row is not null)
        {
            AddError(row, ShiftImportIssueCodes.DuplicateRow,
                $"Duplicate of row {duplicate.Row.RowNumber}.",
                duplicate.Row.RowNumber);
            return;
        }

        if (existingShifts.Any(existing => IsSameShift(existing, shift)))
        {
            AddError(row, ShiftImportIssueCodes.AlreadyScheduled,
                "This assignment is already in the schedule.");
            return;
        }

        var employee = employees.First(employee => employee.Id == shift.EmployeeId);

        // The rules evaluate a candidate for a shift, as during generation.
        var candidateShift = new Shift(
            shift.Date,
            shift.ShiftType,
            shift.RequiredSkill,
            Shift.SupportedRequiredStaff);

        var context = new SchedulingRuleContext
        {
            Employee = employee,
            Shift = candidateShift,
            PlannedShifts = plannedShifts,
            MaxAssignmentsPerEmployee = maxAssignmentsPerEmployee
        };

        foreach (var rule in _rules)
        {
            var result = rule.Evaluate(context);

            if (!result.Success)
            {
                AddError(row, ShiftImportIssueCodes.SchedulingRule,
                    result.FailureReason ?? "A scheduling rule rejected this assignment.");
            }
        }
    }

    private static void AddOpenShiftWarnings(
        ShiftImportRowResult row,
        Shift shift,
        List<(ShiftImportRowResult Row, Shift Shift)> acceptedRows,
        List<Shift> existingShifts)
    {
        // One shift is staffed by one employee, so repeated open rows are how a file asks
        // for several employees. They are allowed, but pointed out.
        var repeated = acceptedRows.FirstOrDefault(accepted => IsSameShift(accepted.Shift, shift));

        if (repeated.Row is not null)
        {
            AddWarning(row, ShiftImportIssueCodes.RepeatedOpenShift,
                $"Same open shift as row {repeated.Row.RowNumber}; each row creates its own shift.",
                repeated.Row.RowNumber);
        }

        if (existingShifts.Any(existing => IsSameShift(existing, shift)))
        {
            AddWarning(row, ShiftImportIssueCodes.OpenShiftExists,
                "An identical open shift is already in the schedule; this row adds another one.");
        }
    }

    private static bool IsSameShift(Shift left, Shift right)
    {
        return left.Date == right.Date &&
               left.ShiftType == right.ShiftType &&
               string.Equals(left.RequiredSkill, right.RequiredSkill, StringComparison.Ordinal) &&
               left.EmployeeId == right.EmployeeId;
    }

    private static void AddError(
        ShiftImportRowResult row,
        string code,
        string message,
        int? relatedRowNumber = null)
    {
        row.Errors.Add(new ShiftImportIssue
        {
            Code = code,
            Message = message,
            RelatedRowNumber = relatedRowNumber
        });
    }

    private static void AddWarning(
        ShiftImportRowResult row,
        string code,
        string message,
        int? relatedRowNumber = null)
    {
        row.Warnings.Add(new ShiftImportIssue
        {
            Code = code,
            Message = message,
            RelatedRowNumber = relatedRowNumber
        });
    }
}
