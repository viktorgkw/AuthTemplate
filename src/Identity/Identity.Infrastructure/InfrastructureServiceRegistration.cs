using Identity.Application.Contracts;
using Identity.Domain.Configuration.Identity;
using Identity.Domain.Entities;
using Identity.Infrastructure.Persistence;
using Identity.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Identity.Infrastructure;

public static class InfrastructureServiceRegistration
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAllServices();
        services.AddPostgreSQL(configuration);
        services.ConfigureIdentity(configuration);

        return services;
    }

    private static IServiceCollection AddAllServices(this IServiceCollection services)
    {
        services.AddScoped<IAuthenticationService, AuthenticationService>();

        return services;
    }

    private static IServiceCollection AddPostgreSQL(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ProfilesDbContext>(config =>
        {
            config.UseNpgsql(configuration.GetConnectionString("PostgreConnectionString"));
        }, ServiceLifetime.Transient);

        return services;
    }

    private static IServiceCollection ConfigureIdentity(this IServiceCollection services, IConfiguration configuration)
    {
        var identityConfig = configuration
            .GetSection(nameof(IdentityConfiguration))
            .Get<IdentityConfiguration>();

        services
            .AddIdentityCore<ApplicationUser>(config =>
             {
                 // Password settings
                 config.Password.RequireDigit = identityConfig.Password.RequireDigit;
                 config.Password.RequireLowercase = identityConfig.Password.RequireLowercase;
                 config.Password.RequireUppercase = identityConfig.Password.RequireUppercase;
                 config.Password.RequireNonAlphanumeric = identityConfig.Password.RequireNonAlphanumeric;
                 config.Password.RequiredLength = identityConfig.Password.RequiredLength;
                 config.Password.RequiredUniqueChars = identityConfig.Password.RequiredUniqueChars;

                 // Lockout settings
                 config.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(identityConfig.Lockout.DefaultLockoutTimeSpanMinutes);
                 config.Lockout.MaxFailedAccessAttempts = identityConfig.Lockout.MaxFailedAccessAttempts;
                 config.Lockout.AllowedForNewUsers = identityConfig.Lockout.AllowedForNewUsers;

                 // User settings
                 config.User.RequireUniqueEmail = identityConfig.User.RequireUniqueEmail;

                 // SignIn settings
                 config.SignIn.RequireConfirmedEmail = identityConfig.SignIn.RequireConfirmedEmail;
                 config.SignIn.RequireConfirmedPhoneNumber = identityConfig.SignIn.RequireConfirmedPhoneNumber;
             })
            .AddRoles<ApplicationRole>()
            .AddEntityFrameworkStores<ProfilesDbContext>();

        return services;
    }
}
