using Azure.Storage.Blobs;
using oliejournal.data.Entities;
using oliejournal.lib.Services.Models;
using System.Diagnostics;

namespace oliejournal.lib.Processes.JournalProcess;

public interface IJournalEntryTranscribeUnit
{
    void Cleanup(string localFile);
    Task CreateJournalTranscript(int journalEntryId, OlieTranscribeResult result, Stopwatch stopwatch, CancellationToken ct);
    Task DeleteJournalEntry(int journalEntryId, CancellationToken ct);
    Task EnsureGoogleLimit(int limit, CancellationToken ct);
    Task<string> GetAudioFile(string blobPath, BlobContainerClient client, CancellationToken ct);
    Task<JournalEntryEntity> GetJournalEntryOrThrow(int journalEntryId, CancellationToken ct);
    Task<bool> IsAlreadyTranscribed(int journalEntryId, CancellationToken ct);
    Task<OlieTranscribeResult> Transcribe(string localFile, CancellationToken ct);
}
