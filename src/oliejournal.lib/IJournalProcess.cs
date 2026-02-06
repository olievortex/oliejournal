using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;

namespace oliejournal.lib;

public interface IJournalProcess
{
    Task<int> IngestAudioEntry(string userId, Stream audio, ServiceBusSender sender, BlobContainerClient client, CancellationToken ct);
}
