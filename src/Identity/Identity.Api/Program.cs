using Identity.Api;
using Identity.Application;
using Identity.Infrastructure;
using Identity.Infrastructure.Extensions;
using SharedKernel.Extensions;

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

app.UseTelemetry();

if (app.Environment.IsDevelopment())
{
    app.UseCors(builder => builder
        .AllowAnyOrigin()
        .AllowAnyHeader()
        .AllowAnyMethod());

    await app.ApplyMigrations();
    await app.InitializeRoles();
}

app.Run();
