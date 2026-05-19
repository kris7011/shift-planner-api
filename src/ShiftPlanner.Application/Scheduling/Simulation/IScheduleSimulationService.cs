using ShiftPlanner.Domain.Employees;
using ShiftPlanner.Domain.Shifts;

namespace ShiftPlanner.Application.Scheduling.Simulation;

public interface IScheduleSimulationService
{
    SimulateScheduleResponse Simulate(
        SimulateScheduleRequest request,
        List<Employee> employees,
        List<Shift> existingShifts);
}