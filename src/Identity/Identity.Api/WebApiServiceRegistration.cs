using SharedKernel.Extensions;

namespace Identity.Api;

public static class WebApiServiceRegistration
{
    public static IServiceCollection AddWebApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers();

        services
            .AddSecurity(configuration)
            .AddGlobalExceptionHandler()
            .ConfigurePostgreHealthChecks(configuration);

        return services;
    }
}
