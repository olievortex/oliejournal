using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using oliejournal.cli.Commands;
using oliejournal.data;
using oliejournal.lib;
using oliejournal.lib.Services;

namespace oliejournal.cli;

public class Program
{
    private static readonly InMemoryChannel _channel = new();
    private static ServiceProvider _serviceProvider = new ServiceCollection().BuildServiceProvider();
    private static IHost _host = Host.CreateDefaultBuilder().Build();

    private static int Main(string[] args)
    {
        AddBaseServices();
        AddHostServices();

        var exitCode = 0;
        var logger = CreateLogger<Program>();
        var ct = CancellationToken.None;

        try
        {
            Console.WriteLine($"{DateTime.UtcNow:u} oliejournal.cli");
            logger.LogInformation("{timeStamp} oliejournal.cli", DateTime.UtcNow.ToString("u"));

            var t = MainAsync(args, ct);
            t.Wait();
            exitCode = t.Result;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
            logger.LogError(ex, "{timeStamp} oliejournal.cli error: {error}", DateTime.UtcNow.ToString("u"), ex);
            exitCode = 1;
        }
        finally
        {
            Console.WriteLine($"{DateTime.UtcNow:u} oliejournal.cli exit: {exitCode}");
            logger.LogInformation("{timeStamp} oliejournal.cli exit: {exitCode}", DateTime.UtcNow.ToString("u"), exitCode);

            FlushChannel();
        }

        return exitCode;
    }

    private static async Task<int> MainAsync(string[] args, CancellationToken ct)
    {
        var olieArgs = new OlieArgs(args);
        var olieConfig = CreateService<IOlieConfig>();
        var scopeFactory = _host.Services.GetRequiredService<IServiceScopeFactory>();

        return olieArgs.Command switch
        {
            OlieArgs.CommandsEnum.AudioProcessQueue => await new CommandAudioProcessQueue(scopeFactory, olieConfig).Run(ct),
            _ => throw new ArgumentException($"The command {olieArgs.Command} is not implemented yet."),
        };
    }

    private static ILogger<T> CreateLogger<T>()
    {
        return _serviceProvider.GetRequiredService<ILogger<T>>();
    }

    private static T CreateService<T>() where T : notnull
    {
        return _serviceProvider.GetRequiredService<T>();
    }

    private static void AddBaseServices()
    {
        var services = new ServiceCollection();
        var config = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .AddUserSecrets<Program>()
            .Build();
        var olieConfig = new OlieConfig(config);

        services.AddLogging(builder =>
        {
            var connectionString = olieConfig.ApplicationInsightsConnectionString;

            // Only Application Insights is registered as a logger provider
            builder.AddApplicationInsights(
                configureTelemetryConfiguration: (config) => config.ConnectionString = connectionString,
                configureApplicationInsightsLoggerOptions: (options) => { }
            );
        });
        services.Configure<TelemetryConfiguration>(config => config.TelemetryChannel = _channel);
        services.AddSingleton(_ => (IConfiguration)config);
        services.AddSingleton<IOlieConfig, OlieConfig>();

        _serviceProvider = services.BuildServiceProvider();
    }

    private static void AddHostServices()
    {
        var configuration = _serviceProvider.GetRequiredService<IConfiguration>();
        var olieConfig = new OlieConfig(configuration);

        var host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                #region Entity Framework

                services.AddDbContext<MyContext>(options =>
                {
                    var connectionString = olieConfig.MySqlConnection;

                    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
                }, ServiceLifetime.Scoped);

                #endregion

                #region Miscellaneous

                services.AddSingleton(_ => configuration);
                services.AddSingleton<IOlieConfig, OlieConfig>();
                services.AddScoped<IMyRepository, MyRepository>();
                services.AddScoped<IJournalBusiness, JournalBusiness>();
                services.AddScoped<IJournalProcess, JournalProcess>();
                services.AddScoped<IOlieService, OlieService>();
                services.AddScoped<IOlieWavReader, OlieWavReader>();

                #endregion
            })
            .Build();

        _host = host;
    }

    private static void FlushChannel()
    {
        // Explicitly call Flush() followed by Delay, as required in console apps.
        // This ensures that even if the application terminates, telemetry is sent to the back end.
        _channel.Flush();

        var t = Task.Delay(TimeSpan.FromMilliseconds(1000));
        t.Wait();
    }
}
