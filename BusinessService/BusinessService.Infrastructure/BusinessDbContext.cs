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
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasMany(u => u.Validations)
            .WithOne(v => v.User)
            .HasForeignKey(v => v.UserId);

        modelBuilder.Entity<Validation>()
            .HasMany(v => v.Steps)
            .WithOne(s => s.Validation)
            .HasForeignKey(s => s.ValidationId);
    }
}
