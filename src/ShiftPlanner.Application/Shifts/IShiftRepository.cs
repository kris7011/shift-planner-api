using ShiftPlanner.Domain.Shifts;

namespace ShiftPlanner.Application.Shifts;

public interface IShiftRepository
{
    Task<Shift> CreateAsync(Shift shift);
    Task<Shift> UpdateAsync(Shift shift);
    Task<List<Shift>> GetAllAsync();
    Task<List<Shift>> GetByEmployeeIdAsync(Guid employeeId);
    Task DeleteAllAsync();
}