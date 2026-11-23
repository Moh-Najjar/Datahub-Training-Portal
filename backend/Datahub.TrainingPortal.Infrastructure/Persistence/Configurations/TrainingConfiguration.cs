using Datahub.TrainingPortal.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Datahub.TrainingPortal.Infrastructure.Persistence.Configurations;

public class TrainingConfiguration : IEntityTypeConfiguration<Training>
{
    public void Configure(EntityTypeBuilder<Training> builder)
    {
        builder.Property(t => t.Title).IsRequired().HasMaxLength(200);
        builder.Property(t => t.Description).IsRequired();
        builder.Property(t => t.Status).HasConversion<string>().HasMaxLength(20);

        builder.HasOne(t => t.Department)
            .WithMany(d => d.Trainings)
            .HasForeignKey(t => t.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.Trainer)
            .WithMany(u => u.TrainingsAuthored)
            .HasForeignKey(t => t.TrainerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
