using HealthChecks.UI.Client;
using Identity.Api;
using Identity.Application;
using Identity.Infrastructure;
using Identity.Infrastructure.Extensions;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var configuration = builder.Configuration;

services.AddWebApiServices(configuration);
services.AddInfrastructureServices(configuration);
services.AddApplicationServices();

var app = builder.Build();

//app.UseExceptionHandler();

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

await app.ApplyMigrations();

app.Run();
