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
using System.Runtime.InteropServices;

namespace oliejournal.cli;

public class Program
{
    private static readonly InMemoryChannel _channel = new();
    private static ServiceProvider _serviceProvider = new ServiceCollection().BuildServiceProvider();
    private static IHost _host = Host.CreateDefaultBuilder().Build();

    private static async Task<int> Main(string[] args)
    {
        AddServices();

        var exitCode = 0;
        var logger = CreateLogger<Program>();
        var cts = new CancellationTokenSource();

        using var sigint = PosixSignalRegistration.Create(PosixSignal.SIGINT, signalContext =>
        {
            cts.Cancel();
            Console.WriteLine($"{DateTime.UtcNow:u} oliejournal.cli - SIGINT detected.");
            signalContext.Cancel = true;
        });

        using var sigterm = PosixSignalRegistration.Create(PosixSignal.SIGTERM, signalContext =>
        {
            cts.Cancel();
            Console.WriteLine($"{DateTime.UtcNow:u} oliejournal.cli - SIGTERM detected.");
            signalContext.Cancel = true;
        });

        try
        {
            Console.WriteLine($"{DateTime.UtcNow:u} oliejournal.cli");
            logger.LogInformation("{timeStamp} oliejournal.cli", DateTime.UtcNow.ToString("u"));

            exitCode = await MainAsync(args, cts.Token);
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

        return olieArgs.Command switch
        {
            OlieArgs.CommandsEnum.AudioProcessQueue => await CreateService<CommandAudioProcessQueue>().Run(ct),
            OlieArgs.CommandsEnum.DeleteOldContent => await CreateService<CommandDeleteOldContent>().Run(ct),
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

    private static void AddServices()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .AddUserSecrets<Program>()
            .Build();
        var config = new OlieConfig(configuration);
        var host = new OlieHost
        {
            ServiceScopeFactory = CreateHostServices(configuration).Services
                .GetRequiredService<IServiceScopeFactory>()
        };

        services.AddLogging(builder =>
        {
            var connectionString = config.ApplicationInsightsConnectionString;

            // Only Application Insights is registered as a logger provider
            builder.AddApplicationInsights(
                configureTelemetryConfiguration: (config) => config.ConnectionString = connectionString,
                configureApplicationInsightsLoggerOptions: (options) => { }
            );
        });
        services.Configure<TelemetryConfiguration>(config => config.TelemetryChannel = _channel);
        services.AddSingleton(_ => (IConfiguration)configuration);
        services.AddSingleton(_ => host);
        services.AddSingleton<IOlieConfig, OlieConfig>();
        services.AddScoped<IOlieService, OlieService>();

        // Commands
        services.AddScoped<CommandAudioProcessQueue>();
        services.AddScoped<CommandDeleteOldContent>();

        _serviceProvider = services.BuildServiceProvider();
    }

    private static IHost CreateHostServices(IConfiguration config)
    {
        var olieConfig = new OlieConfig(config);

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

                services.AddSingleton(_ => config);
                services.AddOlieLibScopes();
            })
            .Build();

        return host;
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
