namespace ShiftPlanner.Infrastructure.Persistence.Employees;

public class EmployeeEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Skills { get; set; } = string.Empty;
}