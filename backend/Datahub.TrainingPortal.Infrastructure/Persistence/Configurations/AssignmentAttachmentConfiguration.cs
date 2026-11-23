using Datahub.TrainingPortal.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Datahub.TrainingPortal.Infrastructure.Persistence.Configurations;

public class AssignmentAttachmentConfiguration : IEntityTypeConfiguration<AssignmentAttachment>
{
    public void Configure(EntityTypeBuilder<AssignmentAttachment> builder)
    {
        builder.Property(a => a.FileName).IsRequired().HasMaxLength(260);
        builder.Property(a => a.StoredFileName).IsRequired().HasMaxLength(260);
        builder.Property(a => a.ContentType).IsRequired().HasMaxLength(150);

        builder.HasOne(a => a.Assignment)
            .WithMany(a => a.Attachments)
            .HasForeignKey(a => a.AssignmentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
