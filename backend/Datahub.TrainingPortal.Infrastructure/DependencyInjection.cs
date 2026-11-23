using Azure.Storage.Blobs;
using Datahub.TrainingPortal.Application.Interfaces;
using Datahub.TrainingPortal.Infrastructure.Auth;
using Datahub.TrainingPortal.Infrastructure.Persistence;
using Datahub.TrainingPortal.Infrastructure.Services;
using Datahub.TrainingPortal.Infrastructure.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Datahub.TrainingPortal.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        var jwtSettings = configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
            ?? throw new InvalidOperationException("Jwt configuration section is missing.");
        services.AddSingleton(jwtSettings);

        services.AddHttpContextAccessor();

        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IDepartmentService, DepartmentService>();
        services.AddScoped<ITrainingService, TrainingService>();
        services.AddScoped<IEnrollmentService, EnrollmentService>();
        services.AddScoped<ITrainingMaterialService, TrainingMaterialService>();
        services.AddScoped<IAssignmentService, AssignmentService>();
        services.AddScoped<IAssignmentSubmissionService, AssignmentSubmissionService>();
        services.AddScoped<IEvaluationService, EvaluationService>();

        var azureStorageConnectionString = configuration.GetConnectionString("AzureStorage")
            ?? throw new InvalidOperationException("AzureStorage connection string is missing.");
        var containerName = configuration["AzureStorage:ContainerName"] ?? "training";

        services.AddSingleton(new BlobContainerClient(azureStorageConnectionString, containerName));
        services.AddScoped<IFileStorageService, AzureBlobStorageService>();

        return services;
    }
}
