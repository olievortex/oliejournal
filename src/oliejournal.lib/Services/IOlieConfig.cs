namespace oliejournal.lib.Services;

public interface IOlieConfig
{
    string AudioProcessQueue { get; }
    string BlobContainerUri { get; }
    string MySqlConnection { get; }
    string ServiceBus { get; }
}
