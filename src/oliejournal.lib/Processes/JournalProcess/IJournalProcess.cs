using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;
using oliejournal.lib.Models;

namespace oliejournal.lib.Processes.JournalProcess;

public interface IJournalProcess
{
    Task Voiceover(int journalEntryId, CancellationToken ct);
    Task Chatbot(int journalEntryId, ServiceBusSender sender, CancellationToken ct);
    Task<JournalEntryListModel?> GetEntry(int journalEntryId, string userId, CancellationToken ct);
    Task<List<JournalEntryListModel>> GetEntryList(string userId, CancellationToken ct);
    Task<int> Ingest(string userId, Stream audio, float? latitude, float? longitude, ServiceBusSender sender, BlobContainerClient client, CancellationToken ct);
    Task Transcribe(int id, BlobContainerClient client, ServiceBusSender sender, CancellationToken ct);
    Task<bool> DeleteEntry(int journalEntryId, string userId, BlobContainerClient client, CancellationToken ct);
}
