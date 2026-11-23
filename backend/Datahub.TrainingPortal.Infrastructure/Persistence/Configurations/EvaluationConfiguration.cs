using Datahub.TrainingPortal.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Datahub.TrainingPortal.Infrastructure.Persistence.Configurations;

public class EvaluationConfiguration : IEntityTypeConfiguration<Evaluation>
{
    public void Configure(EntityTypeBuilder<Evaluation> builder)
    {
        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.EvaluatedByUser)
            .WithMany()
            .HasForeignKey(e => e.EvaluatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Enrollment)
            .WithMany()
            .HasForeignKey(e => e.EnrollmentId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
