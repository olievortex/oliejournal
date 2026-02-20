using Microsoft.Extensions.DependencyInjection;
using oliejournal.data;
using oliejournal.lib.Processes.DeleteOldContentProcess;
using oliejournal.lib.Processes.JournalProcess;
using oliejournal.lib.Services;
using System.Diagnostics.CodeAnalysis;

namespace oliejournal.lib;

public static class OlieCommon
{
    public static string Left(this string str, int length)
    {
        return str.Length <= length ? str : str[..length];
    }

    public static float? SafeFloat(this string? str)
    {
        if (string.IsNullOrWhiteSpace(str)) return null;
        if (float.TryParse(str, out float value)) return value;

        return null;
    }

    public static DateTime AsUtc(this DateTime value)
    {
        return new DateTime(value.Ticks, DateTimeKind.Utc);
    }

    [ExcludeFromCodeCoverage]
    public static void AddOlieLibScopes(this IServiceCollection services)
    {
        #region Services

        services.AddSingleton<IOlieConfig, OlieConfig>();
        services.AddScoped<IMyRepository, MyRepository>();
        services.AddScoped<IOlieService, OlieService>();
        services.AddScoped<IOlieWavReader, OlieWavReader>();

        #endregion

        #region JournalProcess Dependencies

        services.AddScoped<IJournalProcess, JournalProcess>();
        services.AddScoped<IJournalEntryChatbotUnit, JournalEntryChatbotUnit>();
        services.AddScoped<IJournalEntryIngestionUnit, JournalEntryIngestionUnit>();
        services.AddScoped<IJournalEntryTranscribeUnit, JournalEntryTranscribeUnit>();
        services.AddScoped<IJournalEntryVoiceoverUnit, JournalEntryVoiceoverUnit>();

        #endregion

        #region DeleteOldContentProcess Dependencies

        services.AddScoped<IMySqlMaintenance, MySqlMaintenance>();

        #endregion

        services.AddScoped<IJournalApiBusiness, JournalApiBusiness>();
        services.AddScoped<IDeleteOldContentProcess, DeleteOldContentProcess>();
    }
}
