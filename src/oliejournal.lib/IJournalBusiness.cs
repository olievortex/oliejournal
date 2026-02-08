using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;
using oliejournal.data.Entities;
using oliejournal.lib.Enums;
using oliejournal.lib.Services;
using oliejournal.lib.Services.Models;
using System.Diagnostics;

namespace oliejournal.lib;

public interface IJournalBusiness
{
    Task<JournalEntryEntity> CreateJournalEntry(string userId, OlieWavInfo olieWavInfo, string path, int length, CancellationToken ct);
    Task CreateJournalMessage(int id, AudioProcessStepEnum step, ServiceBusSender sender, CancellationToken ct);
    Task CreateJournalTranscript(int journalEntryId, OlieTranscribeResult result, Stopwatch stopwatch, CancellationToken ct);
    OlieWavInfo EnsureAudioValidates(byte[] file);
    Task EnsureGoogleLimit(int limit, CancellationToken ct);
    Task<string> GetAudioFile(string blobPath, BlobContainerClient client, CancellationToken ct);
    Task<string> WriteAudioFileToBlob(string localPath, BlobContainerClient client, CancellationToken ct);
    Task<string> WriteAudioFileToTemp(byte[] file, CancellationToken ct);
}
