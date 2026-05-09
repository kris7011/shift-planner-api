using Microsoft.EntityFrameworkCore;
using ShiftPlanner.Application.Shifts;
using ShiftPlanner.Domain.Shifts;
using ShiftPlanner.Infrastructure.Persistence.Shifts;

namespace ShiftPlanner.Infrastructure.Persistence;

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
            EmployeeId = Guid.Empty,
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
            requiredStaff: 1))
        .ToList();
    }
}