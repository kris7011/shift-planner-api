using ShiftPlanner.Domain.Shifts;

namespace ShiftPlanner.Application.Imports;

public enum ShiftImportConfirmationResult
{
    Confirmed,

    /// <summary>Another request confirmed this import first.</summary>
    AlreadyConfirmed,

    /// <summary>Another import with the same file content was confirmed first.</summary>
    DuplicateContent
}

public interface IShiftImportRepository
{
    Task AddAsync(ShiftImport shiftImport);

    Task<ShiftImport?> GetAsync(Guid id);

    Task<ShiftImport?> FindConfirmedByContentHashAsync(string contentHash);

    /// <summary>
    /// Marks the pending import as confirmed and inserts its shifts in one transaction:
    /// either both happen or neither does.
    /// </summary>
    Task<ShiftImportConfirmationResult> ConfirmAsync(
        Guid id,
        IReadOnlyCollection<Shift> shifts,
        DateTimeOffset confirmedAt);

    Task DeleteAllAsync();
}
