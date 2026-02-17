using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;
using oliejournal.data.Entities;
using oliejournal.lib.Enums;
using oliejournal.lib.Services.Models;

namespace oliejournal.lib.Processes.JournalProcess;

public interface IJournalEntryIngestionUnit
{
    string CreateHash(byte[] bytes);
    Task<JournalEntryEntity> CreateJournalEntry(string userId, string path, int length, string hash, float? latitude, float? longitude, OlieWavInfo olieWavInfo, CancellationToken ct);
    Task CreateJournalMessage(int id, AudioProcessStepEnum step, ServiceBusSender sender, CancellationToken ct);
    OlieWavInfo EnsureAudioValidates(byte[] file);
    Task<byte[]> GetBytesFromStream(Stream stream, CancellationToken ct);
    Task<JournalEntryEntity?> GetDuplicateEntry(string userId, string hash, CancellationToken ct);
    Task<string> WriteAudioFileToBlob(string localPath, BlobContainerClient client, CancellationToken ct);
    Task<string> WriteAudioFileToTemp(byte[] file, CancellationToken ct);
}
