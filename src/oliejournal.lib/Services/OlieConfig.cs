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
    public string MySqlConnection => GetString("OlieMySqlConnection");
    public string ServiceBus => GetString("OlieServiceBus");

    private string GetString(string key)
    {
        return config[key] ?? throw new ApplicationException($"{key} setting missing from configuration");
    }
}
