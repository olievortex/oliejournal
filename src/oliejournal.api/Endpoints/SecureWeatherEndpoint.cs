using Microsoft.AspNetCore.Http.HttpResults;
using oliejournal.api.Models;

namespace oliejournal.api.Endpoints;

public static class SecureWeatherEndpoint
{
    public static void MapSecureWeatherForecastEndpoints(this WebApplication app)
    {
        app.MapGet("/api/secure/weatherforecast", GetWeatherForecast).RequireAuthorization().WithName("GetSecureWeatherForecast");
    }

    public static Ok<List<WeatherForecastModel>> GetWeatherForecast()
    {
        var summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        var forecast = Enumerable.Range(1, 5).Select(index =>
            new WeatherForecastModel
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = summaries[Random.Shared.Next(summaries.Length)]
            })
            .ToList();

        return TypedResults.Ok(forecast);
    }
}
