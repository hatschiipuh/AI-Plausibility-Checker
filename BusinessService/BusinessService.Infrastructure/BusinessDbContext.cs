using BusinessService.Domain;
using Microsoft.EntityFrameworkCore;

namespace BusinessService.Infrastructure;

public class BusinessDbContext : DbContext
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Validation> Validations => Set<Validation>();
    public DbSet<ValidationStep> ValidationSteps => Set<ValidationStep>();

    public BusinessDbContext(DbContextOptions<BusinessDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Apply all IEntityTypeConfiguration implementations in this assembly.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BusinessDbContext).Assembly);
    }
}
