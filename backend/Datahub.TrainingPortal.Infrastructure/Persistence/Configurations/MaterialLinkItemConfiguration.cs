using Datahub.TrainingPortal.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Datahub.TrainingPortal.Infrastructure.Persistence.Configurations;

public class MaterialLinkItemConfiguration : IEntityTypeConfiguration<MaterialLinkItem>
{
    public void Configure(EntityTypeBuilder<MaterialLinkItem> builder)
    {
        builder.Property(l => l.Title).HasMaxLength(200);
        builder.Property(l => l.Url).IsRequired().HasMaxLength(2048);

        builder.HasOne(l => l.Material)
            .WithMany(m => m.LinkItems)
            .HasForeignKey(l => l.MaterialId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
