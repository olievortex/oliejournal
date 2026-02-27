using Microsoft.Extensions.Configuration;
using System.Diagnostics.CodeAnalysis;

namespace oliejournal.lib.Services;

[ExcludeFromCodeCoverage]
public class OlieConfig(IConfiguration config) : IOlieConfig
{
    public string ApplicationInsightsConnectionString => GetString("APPLICATIONINSIGHTS_CONNECTION_STRING");
    public string AudioProcessQueue => GetString("OlieAudioProcessQueue");
    public string BlobContainerUri => GetString("OlieBlobContainerUri");
    public string ChatbotInstructions => GetString("OlieChatbotInstructions");
    public bool DillonMode => GetOptionalBool("OlieDillonMode");
    public string FfmpegPath => GetString("OlieFfmpegPath");
    public string GoldPath => GetString("OlieGoldPath");
    public string GoogleVoiceName => GetString("OlieGoogleVoiceName");
    public Uri MySqlBackupContainer => new(GetString("OlieMySqlBackupContainer"));
    public string MySqlBackupPath => GetString("OlieMySqlBackupPath");
    public string MySqlConnection => GetString("OlieMySqlConnection");
    public string OpenAiApiKey => GetString("OlieOpenAiApiKey");
    public string OpenAiModel => GetString("OlieOpenAiModel");
    public string ServiceBus => GetString("OlieServiceBus");

    private string GetString(string key)
    {
        return config[key] ?? throw new ApplicationException($"{key} setting missing from configuration");
    }

    private bool GetOptionalBool(string key)
    {
        if (bool.TryParse(config[key], out bool value)) return value;
        return false;
    }
}
