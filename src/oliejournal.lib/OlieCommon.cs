using Microsoft.Extensions.DependencyInjection;
using oliejournal.data;
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

        services.AddScoped<IJournalApiBusiness, JournalApiBusiness>();
    }
}
