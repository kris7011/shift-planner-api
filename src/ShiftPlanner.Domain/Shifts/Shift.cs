using ShiftPlanner.Domain.Employees;

namespace ShiftPlanner.Domain.Shifts;

public class Shift
{
    /// <summary>
    /// The scheduling model staffs each shift with exactly one employee: a persisted shift has a
    /// single <see cref="EmployeeId"/>, schedule generation assigns one employee per open shift and
    /// coverage counts a shift as covered when it has an employee. A need for several employees is
    /// modelled as several shifts.
    /// </summary>
    public const int SupportedRequiredStaff = 1;

    public Guid Id { get; private set; }
    public Guid? EmployeeId { get; private set; }
    public DateOnly Date { get; private set; }
    public ShiftType ShiftType { get; private set; }
    public string RequiredSkill { get; private set; }
    public int RequiredStaff { get; private set; }
    public List<Employee> AssignedEmployees { get; private set; }

    public Shift(
        DateOnly date,
        ShiftType shiftType,
        string requiredSkill,
        int requiredStaff,
        Guid? employeeId = null)
    {
        Id = Guid.NewGuid();
        Date = date;
        ShiftType = shiftType;
        RequiredSkill = requiredSkill;
        RequiredStaff = requiredStaff;
        EmployeeId = employeeId;
        AssignedEmployees = new List<Employee>();
    }

    private Shift(
    Guid id,
    DateOnly date,
    ShiftType shiftType,
    string requiredSkill,
    int requiredStaff,
    Guid? employeeId)
    {
        Id = id;
        Date = date;
        ShiftType = shiftType;
        RequiredSkill = requiredSkill;
        RequiredStaff = requiredStaff;
        EmployeeId = employeeId;
        AssignedEmployees = new List<Employee>();
    }

    public static Shift FromPersistence(
    Guid id,
    DateOnly date,
    ShiftType shiftType,
    string requiredSkill,
    int requiredStaff,
    Guid? employeeId)
    {
        return new Shift(id, date, shiftType, requiredSkill, requiredStaff, employeeId);
    }

    public static bool IsSupportedRequiredStaff(int requiredStaff)
    {
        return requiredStaff == SupportedRequiredStaff;
    }

    public bool IsFullyStaffed()
    {
        return AssignedEmployees.Count >= RequiredStaff;
    }

    public int MissingStaffCount()
    {
        var missing = RequiredStaff - AssignedEmployees.Count;

        if (missing < 0)
        {
            return 0;
        }

        return missing;
    }

    public bool CanAssign(Employee employee)
    {
        return employee.HasSkill(RequiredSkill);
    }

    public void AssignEmployee(Employee employee)
    {
        if (!CanAssign(employee))
        {
            throw new InvalidOperationException("Employee does not have the required skill.");
        }

        AssignedEmployees.Add(employee);
    }

    public void AssignToEmployee(Guid employeeId)
    {
        EmployeeId = employeeId;
    }
}