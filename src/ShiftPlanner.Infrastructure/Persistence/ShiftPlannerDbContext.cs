using Microsoft.EntityFrameworkCore;
using ShiftPlanner.Infrastructure.Persistence.Employees;
using ShiftPlanner.Infrastructure.Persistence.Imports;
using ShiftPlanner.Infrastructure.Persistence.Shifts;

namespace ShiftPlanner.Infrastructure.Persistence;

public class ShiftPlannerDbContext : DbContext
{
    public ShiftPlannerDbContext(DbContextOptions<ShiftPlannerDbContext> options)
        : base(options)
    {
    }

    public DbSet<EmployeeEntity> Employees => Set<EmployeeEntity>();
    public DbSet<ShiftEntity> Shifts => Set<ShiftEntity>();
    public DbSet<ShiftImportEntity> ShiftImports => Set<ShiftImportEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var shiftImport = modelBuilder.Entity<ShiftImportEntity>();

        shiftImport.ToTable("ShiftImports");
        shiftImport.Property(entity => entity.FileName).HasMaxLength(255).IsRequired();
        shiftImport.Property(entity => entity.ContentHash).HasMaxLength(64).IsRequired();
        shiftImport.Property(entity => entity.Content).IsRequired();
        shiftImport.Property(entity => entity.Status).HasConversion<string>().HasMaxLength(20);

        // At most one confirmed import per file content: a second confirmation of the same
        // file fails in the database even when two requests race.
        shiftImport.HasIndex(entity => entity.ContentHash)
            .IsUnique()
            .HasFilter("\"Status\" = 'Confirmed'");
    }
}