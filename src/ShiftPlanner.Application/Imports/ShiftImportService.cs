using System.Security.Cryptography;
using System.Text;
using ShiftPlanner.Application.Employees;
using ShiftPlanner.Application.Shifts;
using ShiftPlanner.Domain.Shifts;

namespace ShiftPlanner.Application.Imports;

public interface IShiftImportService
{
    /// <summary>
    /// Validates the file and stores it as a pending import. No shift is created.
    /// Returns null together with the reasons when the file itself cannot be read.
    /// </summary>
    Task<(ShiftImportPreviewResponse? Preview, List<string> FileErrors)> PreviewAsync(
        string fileName,
        string content,
        int maxAssignmentsPerEmployee);

    Task<ConfirmShiftImportResult> ConfirmAsync(Guid importId);
}

/// <summary>
/// Two-step import: a preview that only reads and validates, and an explicit confirmation that
/// validates the stored file again against the current schedule before anything is written.
/// </summary>
public class ShiftImportService : IShiftImportService
{
    private readonly IShiftImportRepository _importRepository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IShiftRepository _shiftRepository;
    private readonly ShiftImportValidator _validator;
    private readonly TimeProvider _timeProvider;

    public ShiftImportService(
        IShiftImportRepository importRepository,
        IEmployeeRepository employeeRepository,
        IShiftRepository shiftRepository,
        ShiftImportValidator validator,
        TimeProvider timeProvider)
    {
        _importRepository = importRepository;
        _employeeRepository = employeeRepository;
        _shiftRepository = shiftRepository;
        _validator = validator;
        _timeProvider = timeProvider;
    }

    public async Task<(ShiftImportPreviewResponse? Preview, List<string> FileErrors)> PreviewAsync(
        string fileName,
        string content,
        int maxAssignmentsPerEmployee)
    {
        var parsed = ShiftCsvParser.Parse(content);

        if (parsed.FileErrors.Count > 0)
        {
            return (null, parsed.FileErrors);
        }

        var shiftImport = new ShiftImport
        {
            Id = Guid.NewGuid(),
            FileName = fileName,
            ContentHash = ComputeContentHash(content),
            Content = content,
            MaxAssignmentsPerEmployee = maxAssignmentsPerEmployee,
            Status = ShiftImportStatus.Pending,
            CreatedAt = _timeProvider.GetUtcNow()
        };

        var (preview, _) = await EvaluateAsync(shiftImport, parsed);

        await _importRepository.AddAsync(shiftImport);

        return (preview, []);
    }

    public async Task<ConfirmShiftImportResult> ConfirmAsync(Guid importId)
    {
        var shiftImport = await _importRepository.GetAsync(importId);

        if (shiftImport is null)
        {
            return new ConfirmShiftImportResult
            {
                Outcome = ConfirmShiftImportOutcome.NotFound,
                ImportId = importId,
                Error = "The import was not found."
            };
        }

        if (shiftImport.Status == ShiftImportStatus.Confirmed)
        {
            return AlreadyConfirmed(shiftImport);
        }

        // The schedule may have changed since the preview, so the stored file is checked again.
        var (preview, shifts) = await EvaluateAsync(shiftImport, ShiftCsvParser.Parse(shiftImport.Content));

        if (!preview.CanConfirm)
        {
            return new ConfirmShiftImportResult
            {
                Outcome = ConfirmShiftImportOutcome.NoLongerValid,
                ImportId = importId,
                Error = "The import cannot be confirmed: the file has errors or was already imported. Nothing was saved.",
                Preview = preview
            };
        }

        var confirmedAt = _timeProvider.GetUtcNow();
        var result = await _importRepository.ConfirmAsync(importId, shifts, confirmedAt);

        return result switch
        {
            ShiftImportConfirmationResult.AlreadyConfirmed =>
                AlreadyConfirmed(await _importRepository.GetAsync(importId) ?? shiftImport),
            ShiftImportConfirmationResult.DuplicateContent => new ConfirmShiftImportResult
            {
                Outcome = ConfirmShiftImportOutcome.NoLongerValid,
                ImportId = importId,
                Error = "The same file was imported by another request. Nothing was saved."
            },
            _ => new ConfirmShiftImportResult
            {
                Outcome = ConfirmShiftImportOutcome.Confirmed,
                ImportId = importId,
                CreatedShiftCount = shifts.Count,
                ConfirmedAt = confirmedAt
            }
        };
    }

    private async Task<(ShiftImportPreviewResponse Preview, List<Shift> Shifts)> EvaluateAsync(
        ShiftImport shiftImport,
        ShiftCsvParseResult parsed)
    {
        var employees = await _employeeRepository.GetAllAsync();
        var existingShifts = await _shiftRepository.GetAllAsync();

        var validation = _validator.Validate(
            parsed,
            employees,
            existingShifts,
            shiftImport.MaxAssignmentsPerEmployee);

        var fileErrors = new List<string>();
        var earlierImport = await _importRepository.FindConfirmedByContentHashAsync(shiftImport.ContentHash);

        if (earlierImport is not null && earlierImport.Id != shiftImport.Id)
        {
            fileErrors.Add(
                $"This file was already imported on {earlierImport.ConfirmedAt:yyyy-MM-dd HH:mm} UTC " +
                $"({earlierImport.CreatedShiftCount} shifts). Importing it again would duplicate them.");
        }

        var invalidRows = validation.Rows.Count(row => !row.IsValid);

        var preview = new ShiftImportPreviewResponse
        {
            ImportId = shiftImport.Id,
            FileName = shiftImport.FileName,
            Status = shiftImport.Status,
            CanConfirm = invalidRows == 0 && fileErrors.Count == 0,
            MaxAssignmentsPerEmployee = shiftImport.MaxAssignmentsPerEmployee,
            TotalRows = validation.Rows.Count,
            ValidRows = validation.Rows.Count - invalidRows,
            InvalidRows = invalidRows,
            WarningCount = validation.Rows.Sum(row => row.Warnings.Count),
            OpenShiftCount = validation.Shifts.Count(shift => shift.EmployeeId is null),
            AssignedShiftCount = validation.Shifts.Count(shift => shift.EmployeeId is not null),
            FileErrors = fileErrors,
            Rows = validation.Rows
        };

        return (preview, validation.Shifts);
    }

    private static ConfirmShiftImportResult AlreadyConfirmed(ShiftImport shiftImport)
    {
        return new ConfirmShiftImportResult
        {
            Outcome = ConfirmShiftImportOutcome.AlreadyConfirmed,
            ImportId = shiftImport.Id,
            CreatedShiftCount = shiftImport.CreatedShiftCount,
            ConfirmedAt = shiftImport.ConfirmedAt,
            Error = "The import has already been confirmed; its shifts were created once."
        };
    }

    /// <summary>
    /// Hashes the content with line endings and the byte order mark normalised, so the same
    /// file saved on another system still counts as the same file.
    /// </summary>
    public static string ComputeContentHash(string content)
    {
        var normalised = content
            .TrimStart('﻿')
            .Replace("\r\n", "\n")
            .Replace('\r', '\n')
            .Trim();

        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(normalised)));
    }
}
