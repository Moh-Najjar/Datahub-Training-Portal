using System.Text;
using Azure.Storage.Blobs;
using Datahub.TrainingPortal.Api.Authorization;
using Datahub.TrainingPortal.Api.Middleware;
using Datahub.TrainingPortal.Application;
using Datahub.TrainingPortal.Domain.Constants;
using Datahub.TrainingPortal.Infrastructure;
using Datahub.TrainingPortal.Infrastructure.Auth;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers().AddFluentValidation(fv =>
{
    fv.RegisterValidatorsFromAssembly(typeof(Datahub.TrainingPortal.Application.DependencyInjection).Assembly);
    fv.AutomaticValidationEnabled = true;
});

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

const string FrontendCorsPolicy = "FrontendCorsPolicy";
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();

builder.Services.AddCors(options =>
{
    options.AddPolicy(FrontendCorsPolicy, policy =>
    {
        policy.AllowAnyHeader()
            .AllowAnyMethod();

        if (allowedOrigins is { Length: > 0 })
        {
            policy.WithOrigins(allowedOrigins);
            return;
        }

        if (builder.Environment.IsDevelopment())
        {
            policy.SetIsOriginAllowed(static origin =>
            {
                if (!Uri.TryCreate(origin, UriKind.Absolute, out var uri))
                {
                    return false;
                }

                return uri.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase)
                    || uri.Host.Equals("127.0.0.1", StringComparison.OrdinalIgnoreCase);
            });

            return;
        }

        policy.WithOrigins("http://localhost:5173");
    });
});

var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
    ?? throw new InvalidOperationException("Jwt configuration section is missing.");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key))
    };
});

builder.Services.AddSingleton<IAuthorizationHandler, SameDepartmentHandler>();
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(PolicyNames.SuperAdminOnly, p => p.RequireRole(RoleNames.SuperAdmin));

    options.AddPolicy(PolicyNames.DepartmentManagement, p =>
        p.RequireRole(RoleNames.SuperAdmin, RoleNames.DepartmentAdmin));

    options.AddPolicy(PolicyNames.TrainingAuthoring, p =>
        p.RequireRole(RoleNames.SuperAdmin, RoleNames.DepartmentAdmin));

    options.AddPolicy(PolicyNames.SameDepartment, p => p.Requirements.Add(new SameDepartmentRequirement()));
});

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<Datahub.TrainingPortal.Infrastructure.Persistence.AppDbContext>();
    var passwordHasher = scope.ServiceProvider.GetRequiredService<Datahub.TrainingPortal.Application.Interfaces.IPasswordHasher>();
    await Datahub.TrainingPortal.Infrastructure.Persistence.DbInitializer.SeedAsync(db, passwordHasher);

    var containerClient = scope.ServiceProvider.GetRequiredService<BlobContainerClient>();
    await containerClient.CreateIfNotExistsAsync();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "Datahub Training Portal API");
    });
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseCors(FrontendCorsPolicy);

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
