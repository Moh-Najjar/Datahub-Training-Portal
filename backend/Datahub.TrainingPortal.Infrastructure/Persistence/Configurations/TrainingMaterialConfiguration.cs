using Datahub.TrainingPortal.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Datahub.TrainingPortal.Infrastructure.Persistence.Configurations;

public class TrainingMaterialConfiguration : IEntityTypeConfiguration<TrainingMaterial>
{
    public void Configure(EntityTypeBuilder<TrainingMaterial> builder)
    {
        builder.Property(m => m.Type).HasConversion<string>().HasMaxLength(20);
        builder.Property(m => m.Title).IsRequired().HasMaxLength(200);

        builder.HasOne(m => m.Training)
            .WithMany(t => t.Materials)
            .HasForeignKey(m => m.TrainingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(m => m.UploadedByUser)
            .WithMany()
            .HasForeignKey(m => m.UploadedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
