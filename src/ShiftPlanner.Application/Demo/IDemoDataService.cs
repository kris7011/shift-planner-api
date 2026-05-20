namespace ShiftPlanner.Application.Demo;

public interface IDemoDataService
{
    Task<DemoSeedResult> SeedAsync();
}