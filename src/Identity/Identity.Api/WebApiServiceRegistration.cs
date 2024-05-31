using Microsoft.OpenApi.Models;
using SharedKernel.Http;

namespace Identity.Api;

public static class WebApiServiceRegistration
{
    public static IServiceCollection AddWebApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers();
        services.AddAndConfigureSwagger();
        services.ConfigurePostgreHealthChecks(configuration);

        services
            .AddExceptionHandler<GlobalExceptionHandler>()
            .AddProblemDetails();

        return services;
    }

    private static IServiceCollection ConfigurePostgreHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddHealthChecks()
            .AddNpgSql(configuration.GetConnectionString("PostgreConnectionString"));

        return services;
    }

    private static IServiceCollection AddAndConfigureSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.CustomSchemaIds((Type x) => x.FullName);

            options.AddSecurityDefinition(
                "Bearer",
                new OpenApiSecurityScheme
                {
                    Name = "Identity",
                    Description = "Example: Bearer [TOKEN]",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey
                });

            options.AddSecurityRequirement(
                new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Id = "Bearer",
                                Type = ReferenceType.SecurityScheme
                            }
                        },
                        new List<string>()
                    }
                });
        });

        return services;
    }
}
