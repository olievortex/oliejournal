using Azure.Monitor.OpenTelemetry.AspNetCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.HttpOverrides;
using oliejournal.lib.Services;
using oliejournal.web.Pages;
using System.Net;

namespace oliejournal.web;

public static class Program
{
    private const string OlieBlue = "blue";

    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var config = builder.AddConfiguration();
        builder.AddHttpClient(config);
        builder.Services.AddRazorPages();
        builder.Services.AddOpenTelemetry().UseAzureMonitor();
        builder.AddOidcAuthentication();
        builder.AddReverseProxySupport();

        var app = builder.Build();
        app.UseForwardedHeaders();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapStaticAssets();
        app.MapRazorPages()
           .WithStaticAssets();

        app.Run();
    }

    private static void AddReverseProxySupport(this WebApplicationBuilder builder)
    {
        builder.Services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders =
                ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost;
            options.KnownProxies.Add(IPAddress.Parse("127.0.0.1"));
            options.KnownProxies.Add(IPAddress.Parse("::1"));
        });
    }

    private static void AddOidcAuthentication(this WebApplicationBuilder builder)
    {
        builder.Services.AddAuthentication(options =>
        {
            options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
        })
            .AddCookie()
            .AddOpenIdConnect(options =>
            {
                options.Scope.Add("profile");
                options.Scope.Add("email");
                options.SaveTokens = true;

                // Request the API audience
                options.Events = new OpenIdConnectEvents
                {
                    OnRedirectToIdentityProvider = context =>
                    {
                        context.ProtocolMessage.SetParameter("audience", "https://oliejournal.olievortex.com");
                        return Task.CompletedTask;
                    }
                };

                // Validate the audience in received tokens
                options.TokenValidationParameters.ValidAudiences = ["https://oliejournal.olievortex.com"];
            });
    }

    private static void AddHttpClient(this WebApplicationBuilder builder, OlieConfig config)
    {
        builder.Services.AddHttpClient(OlieBlue, httpClient =>
        {
            httpClient.BaseAddress = new Uri(config.BlueUrl);
        })
            .ConfigurePrimaryHttpMessageHandler(config => new HttpClientHandler
            {
                AutomaticDecompression = System.Net.DecompressionMethods.Brotli | System.Net.DecompressionMethods.GZip
            });
    }

    private static OlieConfig AddConfiguration(this WebApplicationBuilder builder)
    {
        builder.Configuration
            .AddEnvironmentVariables()
            .AddUserSecrets<DeleteAccountModel>()
            .Build();

        return new OlieConfig(builder.Configuration);
    }

    public static HttpClient GetOlieBlue(IHttpClientFactory httpClientFactory)
    {
        return httpClientFactory.CreateClient(OlieBlue);
    }
}
