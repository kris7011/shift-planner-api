namespace ShiftPlanner.Domain.Employees;

public class Employee
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public List<string> Skills { get; private set; }

    public Employee(string name, List<string> skills)
    {
        Id = Guid.NewGuid();
        Name = name;
        Skills = skills;
    }

    private Employee(Guid id, string name, List<string> skills)
    {
        Id = id;
        Name = name;
        Skills = skills;
    }

    public static Employee FromPersistence(Guid id, string name, List<string> skills)
    {
        return new Employee(id, name, skills);
    }

    public bool HasSkill(string skill)
    {
        return Skills.Contains(skill);
    }
}