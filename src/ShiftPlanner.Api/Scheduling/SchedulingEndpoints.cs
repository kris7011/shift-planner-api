using ShiftPlanner.Application.Employees;
using ShiftPlanner.Application.Scheduling;
using ShiftPlanner.Application.Scheduling.Models;
using ShiftPlanner.Application.Scheduling.Overview;
using ShiftPlanner.Application.Shifts;

namespace ShiftPlanner.Api.Scheduling;

public static class SchedulingEndpoints
{
    public static void MapSchedulingEndpoints(this WebApplication app)
    {
        app.MapPost("/api/schedule/generate", async (
            GenerateScheduleRequest request,
            IScheduleGeneratorService scheduleGeneratorService,
            IEmployeeRepository employeeRepository,
            IShiftRepository shiftRepository) =>
        {
            if (request.MaxAssignmentsPerEmployee <= 0)
            {
                return Results.BadRequest(
                    "MaxAssignmentsPerEmployee must be greater than 0.");
            }

            var employees = await employeeRepository.GetAllAsync();
            var shifts = await shiftRepository.GetAllAsync();

            var schedule = scheduleGeneratorService.Generate(
                employees,
                openShifts: shifts.Where(shift => shift.EmployeeId == null).ToList(),
                existingShifts: shifts.Where(shift => shift.EmployeeId != null).ToList(),
                maxAssignmentsPerEmployee: request.MaxAssignmentsPerEmployee);

            foreach (var assignment in schedule.Where(x => x.WasAssigned && x.EmployeeId.HasValue))
            {
                var shift = shifts.First(x => x.Id == assignment.ShiftId);

                var employeeId = assignment.EmployeeId.GetValueOrDefault();

                shift.AssignToEmployee(employeeId);

                await shiftRepository.UpdateAsync(shift);
            }

            var response = new GenerateScheduleResponse
            {
                Message = "Schedule generation completed.",
                EmployeeCount = employees.Count,
                ShiftCount = shifts.Count,
                Assignments = schedule
                    .Select(assignment => new ScheduleAssignmentResponse
                    {
                        ShiftId = assignment.ShiftId,
                        EmployeeId = assignment.EmployeeId,
                        EmployeeName = assignment.EmployeeName,
                        RequiredSkill = assignment.RequiredSkill,
                        WasAssigned = assignment.WasAssigned,
                        FailureReasons = assignment.FailureReasons
                    })
                    .ToList()
            };

            return Results.Ok(response);
        })
        .WithName("GenerateSchedule")
        .WithSummary("Generates schedule assignments")
        .WithDescription("Generates employee assignments for open shifts using skill matching, workload balancing, scheduling rules, and failure reasons.")
        .Produces<GenerateScheduleResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest);


        app.MapGet("/api/schedule/overview", async (
            IScheduleGeneratorService scheduleGeneratorService,
            IScheduleOverviewService scheduleOverviewService,
            IEmployeeRepository employeeRepository,
            IShiftRepository shiftRepository,
            IEmployeeLoadStatusService employeeLoadStatusService,
            IEmployeeLoadService employeeLoadService) =>
        {
            var employees = await employeeRepository.GetAllAsync();
            var shifts = await shiftRepository.GetAllAsync();

            var openShifts = shifts
                .Where(shift => shift.EmployeeId == null)
                .ToList();

            var existingShifts = shifts
                .Where(shift => shift.EmployeeId != null)
                .ToList();

            var scheduleResults = scheduleGeneratorService.Generate(
                employees,
                openShifts,
                existingShifts,
                maxAssignmentsPerEmployee: 5);

            var highRiskEmployeeCount = employees.Count(employee =>
            {
                var employeeShifts = shifts
                    .Where(shift => shift.EmployeeId == employee.Id)
                    .ToList();

                var load = employeeLoadService.CalculateLoad(employeeShifts);
                var status = employeeLoadStatusService.CalculateStatus(load);

                return status == Domain.Employees.LoadStatus.High;
            });

            var response = scheduleOverviewService.CreateOverview(
                employees: employees,
                highRiskEmployeeCount: highRiskEmployeeCount,
                shifts: shifts,
                scheduleResults: scheduleResults);

            return Results.Ok(response);
        })
        .WithName("GetScheduleOverview")
        .WithSummary("Gets schedule overview")
        .WithDescription("Returns leadership-oriented schedule overview with coverage, unassigned shifts, workload risk, and scheduling failure reasons.")
        .Produces<ScheduleOverviewResponse>(StatusCodes.Status200OK);
    }
}