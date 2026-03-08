using BusinessService.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BusinessService.Infrastructure.EntityConfigurations;

public class ValidationStepConfiguration : IEntityTypeConfiguration<ValidationStep>
{
    public void Configure(EntityTypeBuilder<ValidationStep> builder)
    {
        builder.HasOne(s => s.Validation)
            .WithMany(v => v.Steps)
            .HasForeignKey(s => s.ValidationId);
    }
}
