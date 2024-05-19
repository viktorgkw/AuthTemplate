using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Identity.Application;

public static class ApplicationServiceRegistration
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.AddAutoMapper(assembly);
        services.AddMediatR(x => x.RegisterServicesFromAssembly(assembly));

        return services;
    }
}
