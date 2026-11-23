using Datahub.TrainingPortal.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Datahub.TrainingPortal.Infrastructure.Persistence.Configurations;

public class AssignmentSubmissionAttachmentConfiguration : IEntityTypeConfiguration<AssignmentSubmissionAttachment>
{
    public void Configure(EntityTypeBuilder<AssignmentSubmissionAttachment> builder)
    {
        builder.Property(a => a.FileName).IsRequired().HasMaxLength(260);
        builder.Property(a => a.StoredFileName).IsRequired().HasMaxLength(260);
        builder.Property(a => a.ContentType).IsRequired().HasMaxLength(150);

        builder.HasOne(a => a.Submission)
            .WithMany(s => s.Attachments)
            .HasForeignKey(a => a.SubmissionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
