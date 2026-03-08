using BusinessService.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BusinessService.Infrastructure.EntityConfigurations;

public class ValidationConfiguration : IEntityTypeConfiguration<Validation>
{
    public void Configure(EntityTypeBuilder<Validation> builder)
    {
        builder.HasOne(v => v.User)
            .WithMany(u => u.Validations)
            .HasForeignKey(v => v.UserId);

        builder.HasMany(v => v.Steps)
            .WithOne(s => s.Validation)
            .HasForeignKey(s => s.ValidationId);
    }
}
