using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using Prometheus;
using SharedKernel.Configuration;
using SharedKernel.Http;
using SharedKernel.MediatR;
using SharedKernel.Models;
using SharedKernel.Models.Interfaces;
using System.Reflection;
using System.Text;

namespace SharedKernel.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Configures everything needed for OpenTelemetry, Grafana and Prometheus.
    /// </summary>
    public static IServiceCollection AddOPTL(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddOpenTelemetry()
            .WithMetrics(opt => opt
                .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("Identity.Api"))
                .AddMeter("m2vira")
                .AddAspNetCoreInstrumentation()
                .AddRuntimeInstrumentation()
                .AddProcessInstrumentation()
                .AddOtlpExporter(opts =>
                {
                    opts.Endpoint = new Uri(configuration["Optl:Endpoint"]);
                }));

        return services;
    }

    /// <summary>
    /// Adds <see cref="RequestCorrelationId"/>.
    /// </summary>
    public static IServiceCollection AddRequestCorellationId(this IServiceCollection services)
        => services.AddScoped<RequestCorrelationId>();

    /// <summary>
    /// Adds AutoMapper and MediatR with <see cref="LoggingBehavior{TRequest, TResponse}"/> that logs the request and response with the elapsed seconds.
    /// </summary>
    public static IServiceCollection AddMapperAndMediatR(this IServiceCollection services, Assembly assembly)
    {
        services.AddAutoMapper(assembly);
        services.AddMediatR(x => x.RegisterServicesFromAssembly(assembly));

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));

        return services;
    }

    /// <summary>
    /// Configures the <see cref="JwtConfig"/> and adds Authentication with JWT Bearer token.
    /// </summary>
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

    /// <summary>
    /// Configures the <see cref="GlobalExceptionHandler"/> and adds problem details.
    /// </summary>
    public static IServiceCollection AddGlobalExceptionHandler(this IServiceCollection services)
    {
        services
           .AddExceptionHandler<GlobalExceptionHandler>()
           .AddProblemDetails();

        return services;
    }

    /// <summary>
    /// Adds HealthChecks for NpgSql.
    /// </summary>
    public static IServiceCollection ConfigurePostgreHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddHealthChecks()
            .AddNpgSql(configuration.GetConnectionString("PostgreConnectionString"))
            .ForwardToPrometheus();

        return services;
    }

    /// <summary>
    /// Automatically registers every Service's interface that inherits from <see cref="IService"/>.
    /// </summary>
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
