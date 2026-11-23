using Datahub.TrainingPortal.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Datahub.TrainingPortal.Infrastructure.Persistence.Configurations;

public class MaterialFileItemConfiguration : IEntityTypeConfiguration<MaterialFileItem>
{
    public void Configure(EntityTypeBuilder<MaterialFileItem> builder)
    {
        builder.Property(f => f.Title).HasMaxLength(200);
        builder.Property(f => f.FileName).IsRequired().HasMaxLength(260);
        builder.Property(f => f.StoredFileName).IsRequired().HasMaxLength(260);
        builder.Property(f => f.BlobPath).IsRequired().HasMaxLength(600);
        builder.Property(f => f.ContentType).IsRequired().HasMaxLength(150);

        builder.HasOne(f => f.Material)
            .WithMany(m => m.FileItems)
            .HasForeignKey(f => f.MaterialId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
