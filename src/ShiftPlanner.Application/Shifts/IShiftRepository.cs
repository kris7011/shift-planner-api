using ShiftPlanner.Domain.Shifts;

namespace ShiftPlanner.Application.Shifts;

public interface IShiftRepository
{
    Task<Shift> CreateAsync(Shift shift);
    Task<List<Shift>> GetAllAsync();
}