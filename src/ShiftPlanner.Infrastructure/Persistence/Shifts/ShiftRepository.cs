using Microsoft.EntityFrameworkCore;
using ShiftPlanner.Application.Shifts;
using ShiftPlanner.Domain.Shifts;

namespace ShiftPlanner.Infrastructure.Persistence.Shifts;

public class ShiftRepository : IShiftRepository
{
    private readonly ShiftPlannerDbContext _dbContext;

    public ShiftRepository(ShiftPlannerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Shift> CreateAsync(Shift shift)
    {
        var entity = new ShiftEntity
        {
            Id = shift.Id,
            EmployeeId = shift.EmployeeId ?? Guid.Empty,
            Date = shift.Date,
            ShiftType = shift.ShiftType,
            RequiredSkill = shift.RequiredSkill
        };

        _dbContext.Shifts.Add(entity);

        await _dbContext.SaveChangesAsync();

        return shift;
    }

    public async Task<List<Shift>> GetAllAsync()
    {
        var entities = await _dbContext.Shifts
        .AsNoTracking()
        .ToListAsync();

        return entities
        .Select(entity => Shift.FromPersistence(
            entity.Id,
            entity.Date,
            entity.ShiftType,
            entity.RequiredSkill,
            requiredStaff: 1,
            employeeId: entity.EmployeeId == Guid.Empty ? null : entity.EmployeeId
        ))
        .ToList();
    }

    public async Task<List<Shift>> GetByEmployeeIdAsync(Guid employeeId)
    {
        var entities = await _dbContext.Shifts
            .AsNoTracking()
            .Where(entity => entity.EmployeeId == employeeId)
            .ToListAsync();

        return entities
            .Select(entity => Shift.FromPersistence(
                entity.Id,
                entity.Date,
                entity.ShiftType,
                entity.RequiredSkill,
                requiredStaff: 1,
                employeeId: entity.EmployeeId == Guid.Empty ? null : entity.EmployeeId))
            .ToList();
    }

    public async Task<Shift> UpdateAsync(Shift shift)
    {
        var entity = await _dbContext.Shifts
            .FirstOrDefaultAsync(x => x.Id == shift.Id);

        if (entity == null)
        {
            throw new InvalidOperationException("Shift was not found.");
        }

        entity.EmployeeId = shift.EmployeeId ?? Guid.Empty;
        entity.Date = shift.Date;
        entity.ShiftType = shift.ShiftType;
        entity.RequiredSkill = shift.RequiredSkill;

        await _dbContext.SaveChangesAsync();

        return shift;
    }

    public async Task DeleteAllAsync()
    {
        var shifts = await _dbContext.Shifts.ToListAsync();

        _dbContext.Shifts.RemoveRange(shifts);

        await _dbContext.SaveChangesAsync();
    }
}