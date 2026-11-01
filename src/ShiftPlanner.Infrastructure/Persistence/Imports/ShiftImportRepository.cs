using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using ShiftPlanner.Application.Imports;
using ShiftPlanner.Domain.Shifts;
using ShiftPlanner.Infrastructure.Persistence.Shifts;

namespace ShiftPlanner.Infrastructure.Persistence.Imports;

public class ShiftImportRepository : IShiftImportRepository
{
    private readonly ShiftPlannerDbContext _dbContext;

    public ShiftImportRepository(ShiftPlannerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(ShiftImport shiftImport)
    {
        _dbContext.ShiftImports.Add(new ShiftImportEntity
        {
            Id = shiftImport.Id,
            FileName = shiftImport.FileName,
            ContentHash = shiftImport.ContentHash,
            Content = shiftImport.Content,
            MaxAssignmentsPerEmployee = shiftImport.MaxAssignmentsPerEmployee,
            Status = shiftImport.Status,
            CreatedAt = shiftImport.CreatedAt
        });

        await _dbContext.SaveChangesAsync();
    }

    public async Task<ShiftImport?> GetAsync(Guid id)
    {
        var entity = await _dbContext.ShiftImports
            .AsNoTracking()
            .FirstOrDefaultAsync(entity => entity.Id == id);

        return entity is null ? null : ToModel(entity);
    }

    public async Task<ShiftImport?> FindConfirmedByContentHashAsync(string contentHash)
    {
        var entity = await _dbContext.ShiftImports
            .AsNoTracking()
            .FirstOrDefaultAsync(entity =>
                entity.ContentHash == contentHash &&
                entity.Status == ShiftImportStatus.Confirmed);

        return entity is null ? null : ToModel(entity);
    }

    public async Task<ShiftImportConfirmationResult> ConfirmAsync(
        Guid id,
        IReadOnlyCollection<Shift> shifts,
        DateTimeOffset confirmedAt)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync();

        try
        {
            // Only a pending import can be confirmed. Checking and changing the status in one
            // statement means two simultaneous confirmations cannot both get past this point.
            var updated = await _dbContext.ShiftImports
                .Where(entity => entity.Id == id && entity.Status == ShiftImportStatus.Pending)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(entity => entity.Status, ShiftImportStatus.Confirmed)
                    .SetProperty(entity => entity.ConfirmedAt, confirmedAt)
                    .SetProperty(entity => entity.CreatedShiftCount, shifts.Count));

            if (updated == 0)
            {
                await transaction.RollbackAsync();
                return ShiftImportConfirmationResult.AlreadyConfirmed;
            }

            _dbContext.Shifts.AddRange(shifts.Select(ShiftEntity.FromDomain));
            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            return ShiftImportConfirmationResult.Confirmed;
        }
        catch (Exception exception) when (exception is DbUpdateException or DbException)
        {
            // ExecuteUpdate reports a constraint violation as the provider's DbException,
            // SaveChanges as a DbUpdateException.
            await transaction.RollbackAsync();
            _dbContext.ChangeTracker.Clear();

            if (await IsContentConfirmedElsewhereAsync(id))
            {
                return ShiftImportConfirmationResult.DuplicateContent;
            }

            throw;
        }
    }

    public async Task DeleteAllAsync()
    {
        await _dbContext.ShiftImports.ExecuteDeleteAsync();
    }

    // The unique index on confirmed content hashes rejects the status change when the same
    // file was confirmed through another import.
    private async Task<bool> IsContentConfirmedElsewhereAsync(Guid id)
    {
        var contentHash = await _dbContext.ShiftImports
            .AsNoTracking()
            .Where(entity => entity.Id == id)
            .Select(entity => entity.ContentHash)
            .FirstAsync();

        return await _dbContext.ShiftImports
            .AsNoTracking()
            .AnyAsync(entity =>
                entity.Id != id &&
                entity.ContentHash == contentHash &&
                entity.Status == ShiftImportStatus.Confirmed);
    }

    private static ShiftImport ToModel(ShiftImportEntity entity)
    {
        return new ShiftImport
        {
            Id = entity.Id,
            FileName = entity.FileName,
            ContentHash = entity.ContentHash,
            Content = entity.Content,
            MaxAssignmentsPerEmployee = entity.MaxAssignmentsPerEmployee,
            Status = entity.Status,
            CreatedAt = entity.CreatedAt,
            ConfirmedAt = entity.ConfirmedAt,
            CreatedShiftCount = entity.CreatedShiftCount
        };
    }
}
