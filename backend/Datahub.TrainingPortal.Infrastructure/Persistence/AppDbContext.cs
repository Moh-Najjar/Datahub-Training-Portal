using Datahub.TrainingPortal.Domain.Common;
using Datahub.TrainingPortal.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Datahub.TrainingPortal.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Training> Trainings => Set<Training>();
    public DbSet<TrainingMaterial> TrainingMaterials => Set<TrainingMaterial>();
    public DbSet<MaterialFileItem> MaterialFileItems => Set<MaterialFileItem>();
    public DbSet<MaterialLinkItem> MaterialLinkItems => Set<MaterialLinkItem>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();
    public DbSet<Assignment> Assignments => Set<Assignment>();
    public DbSet<AssignmentAttachment> AssignmentAttachments => Set<AssignmentAttachment>();
    public DbSet<AssignmentSubmission> AssignmentSubmissions => Set<AssignmentSubmission>();
    public DbSet<AssignmentSubmissionAttachment> AssignmentSubmissionAttachments => Set<AssignmentSubmissionAttachment>();
    public DbSet<Evaluation> Evaluations => Set<Evaluation>();
    public DbSet<EvaluationCategoryScore> EvaluationCategoryScores => Set<EvaluationCategoryScore>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(AuditableEntity).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType)
                    .HasQueryFilter(BuildSoftDeleteFilter(entityType.ClrType));
            }
        }
    }

    private static System.Linq.Expressions.LambdaExpression BuildSoftDeleteFilter(Type type)
    {
        var parameter = System.Linq.Expressions.Expression.Parameter(type, "e");
        var property = System.Linq.Expressions.Expression.Property(parameter, nameof(AuditableEntity.IsDeleted));
        var notDeleted = System.Linq.Expressions.Expression.Not(property);
        return System.Linq.Expressions.Expression.Lambda(notDeleted, parameter);
    }
}
