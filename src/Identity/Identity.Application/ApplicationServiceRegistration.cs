using Microsoft.Extensions.DependencyInjection;
using SharedKernel.Extensions;
using System.Reflection;

namespace Identity.Application;

public static class ApplicationServiceRegistration
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddMapperAndMediatR(Assembly.GetExecutingAssembly());

        return services;
    }
}
