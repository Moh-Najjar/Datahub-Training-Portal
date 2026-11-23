using Datahub.TrainingPortal.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Datahub.TrainingPortal.Infrastructure.Persistence.Configurations;

public class EvaluationCategoryScoreConfiguration : IEntityTypeConfiguration<EvaluationCategoryScore>
{
    public void Configure(EntityTypeBuilder<EvaluationCategoryScore> builder)
    {
        builder.Property(c => c.CategoryName).IsRequired().HasMaxLength(200);
        builder.Property(c => c.Criteria).HasMaxLength(1000);

        builder.HasOne(c => c.Evaluation)
            .WithMany(e => e.Categories)
            .HasForeignKey(c => c.EvaluationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
