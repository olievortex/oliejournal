namespace oliejournal.lib.Services;

public interface IOlieConfig
{
    string AudioProcessQueue { get; }
    string BlobContainerUri { get; }
    string ChatbotInstructions { get; }
    string MySqlConnection { get; }
    string OpenAiApiKey { get; }
    string ServiceBus { get; }
}
