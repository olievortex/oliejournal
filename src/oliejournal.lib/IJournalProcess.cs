using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;

namespace oliejournal.lib;

public interface IJournalProcess
{
    Task ChatbotAudioEntry(int journalEntryId, ServiceBusSender sender, CancellationToken ct);
    Task<int> IngestAudioEntry(string userId, Stream audio, ServiceBusSender sender, BlobContainerClient client, CancellationToken ct);
    Task TranscribeAudioEntry(int id, BlobContainerClient client, ServiceBusSender sender, CancellationToken ct);
}
