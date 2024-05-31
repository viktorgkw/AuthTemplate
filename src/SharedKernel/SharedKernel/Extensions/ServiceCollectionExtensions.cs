using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using SharedKernel.Configuration;
using SharedKernel.Http;
using SharedKernel.Models.Interfaces;
using System.Reflection;
using System.Text;

namespace SharedKernel.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMapperAndMediatR(this IServiceCollection services, Assembly assembly)
    {
        services.AddAutoMapper(assembly);
        services.AddMediatR(x => x.RegisterServicesFromAssembly(assembly));

        return services;
    }

    public static IServiceCollection AddSecurity(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtConfig = configuration
            .GetSection(nameof(JwtConfig))
            .Get<JwtConfig>();

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options => options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtConfig.ValidIssuer,
                ValidAudience = jwtConfig.ValidAudience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfig.Secret))
            });

        return services;
    }

    public static IServiceCollection AddGlobalExceptionHandler(this IServiceCollection services)
    {
        services
           .AddExceptionHandler<GlobalExceptionHandler>()
           .AddProblemDetails();

        return services;
    }

    public static IServiceCollection ConfigurePostgreHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddHealthChecks()
            .AddNpgSql(configuration.GetConnectionString("PostgreConnectionString"));

        return services;
    }

    public static IServiceCollection AutoRegisterServices(this IServiceCollection services, Assembly assembly)
    {
        var serviceType = typeof(IService);
        var types = assembly
            .GetTypes()
            .Where(type => serviceType.IsAssignableFrom(type) &&
                type != serviceType &&
                !type.IsAbstract);

        foreach (var type in types)
        {
            var interfaceType = type.GetInterfaces().FirstOrDefault(i => i != serviceType);
            if (interfaceType is not null)
                services.AddScoped(interfaceType, type);
        }

        return services;
    }
}
