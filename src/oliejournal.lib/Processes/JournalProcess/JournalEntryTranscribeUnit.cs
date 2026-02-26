using Azure.Storage.Blobs;
using oliejournal.data;
using oliejournal.data.Entities;
using oliejournal.lib.Services;
using oliejournal.lib.Services.Models;
using System.Diagnostics;

namespace oliejournal.lib.Processes.JournalProcess;

public class JournalEntryTranscribeUnit(IOlieWavReader owr, IOlieService os, IMyRepository repo) : IJournalEntryTranscribeUnit
{
    public void Cleanup(string localFile)
    {
        os.FileDelete(localFile);
    }

    public async Task CreateTranscriptLog(int journalEntryId, OlieTranscribeResult result, Stopwatch stopwatch, CancellationToken ct)
    {
        var entity = new TranscriptLogEntity
        {
            ServiceId = result.ServiceId,
            ProcessingTime = (int)stopwatch.Elapsed.TotalSeconds,
            Cost = result.Cost,
            Exception = result.Exception?.ToString(),
            Created = DateTime.UtcNow,
        };

        await repo.TranscriptLogCreate(entity, ct);
    }

    public async Task<string> GetAudioFile(string blobPath, BlobContainerClient client, CancellationToken ct)
    {
        var localFile = $"{Path.GetTempPath()}{Path.GetFileName(blobPath)}";
        if (os.FileExists(localFile)) return localFile;

        await os.BlobDownloadFile(client, blobPath, localFile, ct);
        return localFile;
    }

    public async Task EnsureGoogleLimit(int limit, CancellationToken ct)
    {
        const int free = 60 * 60; // V1 API
        const double rate = 0.016 / 60; // V1 API w/ data logging

        var lookback = DateTime.UtcNow.AddMonths(-1);
        var billing = await repo.TranscriptLogSummary(lookback, ct);

        if (billing < free) return;

        var cost = (billing - free) * rate;

        if (cost > limit) throw new ApplicationException("Google speech-to-text budget exceeded");
    }

    public async Task<OlieTranscribeResult> Transcribe(string localFile, CancellationToken ct)
    {
        var info = owr.GetOlieWavInfo(localFile);
        return await os.GoogleTranscribeWavNoEx(localFile, info, ct);
    }

    public async Task UpdateEntry(string transcript, JournalEntryEntity entry, CancellationToken ct)
    {
        entry.Transcript = transcript;
        entry.TranscriptCreated = DateTime.UtcNow;

        await repo.JournalEntryUpdate(entry, ct);
    }
}
