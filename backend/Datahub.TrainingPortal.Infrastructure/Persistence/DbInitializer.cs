using Datahub.TrainingPortal.Application.Interfaces;
using Datahub.TrainingPortal.Domain.Constants;
using Datahub.TrainingPortal.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Datahub.TrainingPortal.Infrastructure.Persistence;

public static class DbInitializer
{
    public static async Task SeedAsync(AppDbContext db, IPasswordHasher passwordHasher)
    {
        await db.Database.MigrateAsync();

        if (await db.Users.AnyAsync())
        {
            return;
        }

        db.Users.Add(new User
        {
            FullName = "Super Admin",
            Email = "admin@datahub.local",
            PasswordHash = passwordHasher.Hash("ChangeMe123!"),
            Role = RoleNames.SuperAdmin,
            IsActive = true
        });

        await db.SaveChangesAsync();
    }
}
