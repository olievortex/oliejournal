using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;
using oliejournal.data.Entities;
using oliejournal.lib.Enums;
using oliejournal.lib.Services;

namespace oliejournal.lib.Units;

public interface IJournalEntryIngestionUnit
{
    Task<JournalEntryEntity> CreateJournalEntry(string userId, OlieWavInfo olieWavInfo, string path, int length, CancellationToken ct);
    Task CreateJournalMessage(int id, AudioProcessStepEnum step, ServiceBusSender sender, CancellationToken ct);
    OlieWavInfo EnsureAudioValidates(byte[] file);
    Task<byte[]> GetBytesFromStream(Stream stream, CancellationToken ct);
    Task<string> WriteAudioFileToBlob(string localPath, BlobContainerClient client, CancellationToken ct);
    Task<string> WriteAudioFileToTemp(byte[] file, CancellationToken ct);
}
