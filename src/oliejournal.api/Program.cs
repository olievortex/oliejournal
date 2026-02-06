using Azure.Monitor.OpenTelemetry.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using oliejournal.api.Endpoints;
using oliejournal.api.Models;
using oliejournal.data;
using oliejournal.lib;
using oliejournal.lib.Services;
using OpenTelemetry.Trace;

namespace oliejournal.api;

public static class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var config = builder.AddOlieConfiguration();
        builder.AddOlieAuthentication();
        builder.Services.AddAuthorization();
        builder.AddOlieDependencyInjection();
        builder.AddOlieEntityFramework(config);
        builder.AddOlieTelemetry();

        var app = builder.Build();
        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseOlieEndpoints();
        app.Run();
    }

    private static void AddOlieDependencyInjection(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<IJournalProcess, JournalProcess>();
        builder.Services.AddScoped<IJournalBusiness, JournalBusiness>();
        builder.Services.AddScoped<IOlieWavReader, OlieWavReader>();
        builder.Services.AddScoped<IOlieService, OlieService>();
    }

    private static void AddOlieTelemetry(this WebApplicationBuilder builder)
    {
        builder.Services
            .AddOpenTelemetry()
            .UseAzureMonitor()
            .WithTracing(builder =>
        {
            builder.AddSqlClientInstrumentation();
        });
    }

    private static void AddOlieEntityFramework(this WebApplicationBuilder builder, OlieConfig config)
    {
        var serverVersion = ServerVersion.AutoDetect(config.MySqlConnection);
        void mySqlOptions(DbContextOptionsBuilder options)
        {
            options.UseMySql(config.MySqlConnection, serverVersion);
        }

        builder.Services.AddDbContext<MyContext>(mySqlOptions);
        builder.Services.AddScoped<IMyRepository, MyRepository>();
    }

    private static void AddOlieAuthentication(this WebApplicationBuilder builder)
    {
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                // Kinde uses the 'sub' claim for the user's ID, which maps to Identity.Name
                options.MapInboundClaims = false;
                options.TokenValidationParameters.NameClaimType = "sub";

                options.Authority = "https://antihoistentertainment.kinde.com";
                options.TokenValidationParameters.ValidAudiences = ["https://oliejournal.olievortex.com"];
            });

        builder.Services.AddAuthorization();

    }

    private static OlieConfig AddOlieConfiguration(this WebApplicationBuilder builder)
    {
        builder.Configuration
            .AddEnvironmentVariables()
            .AddUserSecrets<WeatherForecastModel>()
            .Build();

        return new OlieConfig(builder.Configuration);
    }

    private static void UseOlieEndpoints(this WebApplication app)
    {
        app.MapWeatherForecastEndpoints();
        app.MapSecureWeatherForecastEndpoints();
        app.MapJournalEndpoints();
    }
}
