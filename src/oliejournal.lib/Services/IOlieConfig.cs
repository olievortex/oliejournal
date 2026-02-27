namespace oliejournal.lib.Services;

public interface IOlieConfig
{
    string AudioProcessQueue { get; }
    string BlobContainerUri { get; }
    string ChatbotInstructions { get; }
    bool DillonMode { get; }
    string FfmpegPath { get; }
    string GoldPath { get; }
    string GoogleVoiceName { get; }
    Uri MySqlBackupContainer { get; }
    string MySqlBackupPath { get; }
    string MySqlConnection { get; }
    string OpenAiApiKey { get; }
    string OpenAiModel { get; }
    string ServiceBus { get; }
}
