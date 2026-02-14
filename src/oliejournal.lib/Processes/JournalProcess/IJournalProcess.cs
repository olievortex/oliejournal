using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;

namespace oliejournal.lib.Processes.JournalProcess;

public interface IJournalProcess
{
    Task Voiceover(int journalEntryId, CancellationToken ct);
    Task Chatbot(int journalEntryId, ServiceBusSender sender, CancellationToken ct);
    Task<int> Ingest(string userId, Stream audio, ServiceBusSender sender, BlobContainerClient client, CancellationToken ct);
    Task Transcribe(int id, BlobContainerClient client, ServiceBusSender sender, CancellationToken ct);
}
