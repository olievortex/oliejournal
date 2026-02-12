namespace oliejournal.lib.Services;

public interface IOlieConfig
{
    string AudioProcessQueue { get; }
    string BlobContainerUri { get; }
    string ChatbotInstructions { get; }
    string GoldPath { get; }
    string GoogleVoiceName { get; }
    string MySqlConnection { get; }
    string OpenAiApiKey { get; }
    string OpenAiModel { get; }
    string ServiceBus { get; }
}
