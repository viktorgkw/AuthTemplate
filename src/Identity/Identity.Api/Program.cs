using HealthChecks.UI.Client;
using Identity.Api;
using Identity.Application;
using Identity.Infrastructure;
using Identity.Infrastructure.Extensions;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var configuration = builder.Configuration;

services.AddInfrastructureServices(configuration);
services.AddApplicationServices();
services.AddWebApiServices(configuration);

var app = builder.Build();

app.UseExceptionHandler();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

if (app.Environment.IsDevelopment())
{
    await app.ApplyMigrations();
    await app.InitializeRoles();
}

app.Run();
