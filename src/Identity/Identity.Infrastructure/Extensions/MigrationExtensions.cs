using Identity.Infrastructure.Persistence;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Identity.Infrastructure.Extensions;

public static class MigrationExtensions
{
    public static async Task ApplyMigrations(this IApplicationBuilder app)
    {
        using IServiceScope scope = app.ApplicationServices.CreateScope();

        using ProfilesDbContext context = scope.ServiceProvider.GetRequiredService<ProfilesDbContext>();

        await context.Database.MigrateAsync();
    }
}
