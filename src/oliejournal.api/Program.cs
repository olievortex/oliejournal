using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Azure.Monitor.OpenTelemetry.AspNetCore;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using oliejournal.api.Endpoints;
using oliejournal.api.Models;
using oliejournal.data;
using oliejournal.lib;
using oliejournal.lib.Services;
using OpenTelemetry.Trace;
using System.Diagnostics;

namespace oliejournal.api;

public static class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var config = builder.AddOlieConfiguration();
        builder.AddOlieAuthentication();
        builder.Services.AddAuthorization();
        builder.Services.AddOlieLibScopes(config);
        builder.AddOlieEntityFramework(config);
        builder.AddOlieTelemetry();

        var app = builder.Build();
        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseOlieEndpoints();
        app.Run();
    }

    class BadRequestEventListener : IObserver<KeyValuePair<string, object>>, IDisposable
    {
        private readonly IDisposable _subscription;
        private readonly Action<IBadRequestExceptionFeature> _callback;

        public BadRequestEventListener(DiagnosticListener diagnosticListener, Action<IBadRequestExceptionFeature> callback)
        {
            _subscription = diagnosticListener.Subscribe(this!, IsEnabled);
            _callback = callback;
        }
        private static readonly Predicate<string> IsEnabled = (provider) => provider switch
        {
            "Microsoft.AspNetCore.Server.Kestrel.BadRequest" => true,
            _ => false
        };
        public void OnNext(KeyValuePair<string, object> pair)
        {
            if (pair.Value is IFeatureCollection featureCollection)
            {
                var badRequestFeature = featureCollection.Get<IBadRequestExceptionFeature>();

                if (badRequestFeature is not null)
                {
                    _callback(badRequestFeature);
                }
            }
        }
        public void OnError(Exception error) { }
        public void OnCompleted() { }
        public virtual void Dispose() => _subscription.Dispose();
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

    public static ServiceBusSender ServiceBusSender(this IOlieConfig config)
    {
        var client = new ServiceBusClient(config.ServiceBus, new DefaultAzureCredential());
        return client.CreateSender(config.AudioProcessQueue);
    }

    public static BlobContainerClient BlobContainerClient(this IOlieConfig config)
    {
        var blobClient = new BlobContainerClient(new Uri(config.BlobContainerUri), new DefaultAzureCredential());
        return blobClient;
    }
}
