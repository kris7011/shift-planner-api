namespace ShiftPlanner.Application.Imports;

public enum ShiftImportStatus
{
    Pending,
    Confirmed
}

/// <summary>A stored import: the previewed file waiting for, or after, confirmation.</summary>
public class ShiftImport
{
    public Guid Id { get; set; }

    public string FileName { get; set; } = string.Empty;

    /// <summary>SHA-256 of the normalised file content, used to refuse importing the same file twice.</summary>
    public string ContentHash { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public int MaxAssignmentsPerEmployee { get; set; }

    public ShiftImportStatus Status { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? ConfirmedAt { get; set; }

    public int CreatedShiftCount { get; set; }
}

public static class ShiftImportIssueCodes
{
    public const string FieldCount = "FieldCount";
    public const string InvalidDate = "InvalidDate";
    public const string InvalidShiftType = "InvalidShiftType";
    public const string MissingRequiredSkill = "MissingRequiredSkill";
    public const string ValueTooLong = "ValueTooLong";
    public const string UnsafeValue = "UnsafeValue";
    public const string UnknownEmployee = "UnknownEmployee";
    public const string AmbiguousEmployee = "AmbiguousEmployee";
    public const string EmployeeLacksSkill = "EmployeeLacksSkill";
    public const string DuplicateRow = "DuplicateRow";
    public const string AlreadyScheduled = "AlreadyScheduled";
    public const string SchedulingRule = "SchedulingRule";

    // Warnings: the row is valid but worth a second look before confirming.
    public const string RepeatedOpenShift = "RepeatedOpenShift";
    public const string OpenShiftExists = "OpenShiftExists";
    public const string SkillNotHeld = "SkillNotHeld";
}

public class ShiftImportIssue
{
    public string Code { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    /// <summary>The other row involved, for duplicates within the file.</summary>
    public int? RelatedRowNumber { get; set; }
}

public class ShiftImportRowResult
{
    public int RowNumber { get; set; }

    public string Date { get; set; } = string.Empty;

    public string ShiftType { get; set; } = string.Empty;

    public string RequiredSkill { get; set; } = string.Empty;

    public string EmployeeName { get; set; } = string.Empty;

    public Guid? EmployeeId { get; set; }

    public bool IsValid => Errors.Count == 0;

    public List<ShiftImportIssue> Errors { get; set; } = [];

    public List<ShiftImportIssue> Warnings { get; set; } = [];
}

public class ShiftImportPreviewResponse
{
    public Guid ImportId { get; set; }

    public string FileName { get; set; } = string.Empty;

    public ShiftImportStatus Status { get; set; }

    /// <summary>True when every row is valid and the file has not been imported before.</summary>
    public bool CanConfirm { get; set; }

    public int MaxAssignmentsPerEmployee { get; set; }

    public int TotalRows { get; set; }

    public int ValidRows { get; set; }

    public int InvalidRows { get; set; }

    public int WarningCount { get; set; }

    public int OpenShiftCount { get; set; }

    public int AssignedShiftCount { get; set; }

    public List<string> FileErrors { get; set; } = [];

    public List<ShiftImportRowResult> Rows { get; set; } = [];
}

public enum ConfirmShiftImportOutcome
{
    Confirmed,
    NotFound,
    AlreadyConfirmed,
    NoLongerValid
}

public class ConfirmShiftImportResult
{
    public ConfirmShiftImportOutcome Outcome { get; set; }

    public string? Error { get; set; }

    public Guid ImportId { get; set; }

    public int CreatedShiftCount { get; set; }

    public DateTimeOffset? ConfirmedAt { get; set; }

    /// <summary>The fresh validation result when the data changed after the preview.</summary>
    public ShiftImportPreviewResponse? Preview { get; set; }
}
