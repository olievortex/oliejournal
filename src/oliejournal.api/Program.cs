using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using oliejournal.api.Endpoints;

namespace oliejournal.api;

public static class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.AddOlieAuthentication();
        builder.Services.AddAuthorization();

        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();

        var app = builder.Build();
        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapEndpoints();
        app.Run();
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

    private static void MapEndpoints(this WebApplication app)
    {
        app.MapWeatherForecastEndpoints();
        app.MapSecureWeatherForecastEndpoints();
    }
}
