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

    public async Task CreateJournalTranscript(int journalEntryId, OlieTranscribeResult result, Stopwatch stopwatch, CancellationToken ct)
    {
        var entity = new JournalTranscriptEntity
        {
            JournalEntryFk = journalEntryId,
            ServiceFk = result.ServiceId,

            ProcessingTime = (int)stopwatch.Elapsed.TotalSeconds,
            Transcript = result.Transcript?.Left(8096),
            Cost = result.Cost,
            Exception = result.Exception?.ToString().Left(8096),
            Created = DateTime.UtcNow,
        };

        await repo.JournalTranscriptCreate(entity, ct);
    }

    public async Task DeleteJournalEntry(int journalEntryId, CancellationToken ct)
    {
        await repo.JournalTranscriptDelete(journalEntryId, ct);
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
        var billing = await repo.GoogleGetSpeech2TextSummary(lookback, ct);

        if (billing < free) return;

        var cost = (billing - free) * rate;

        if (cost > limit) throw new ApplicationException("Google speech-to-text budget exceeded");
    }

    public async Task<JournalEntryEntity> GetJournalEntryOrThrow(int journalEntryId, CancellationToken ct)
    {
        return await repo.JournalEntryGet(journalEntryId, ct) ??
            throw new ApplicationException($"JournalEntry with {journalEntryId} doesn't exist");
    }

    public async Task<bool> IsAlreadyTranscribed(int journalEntryId, CancellationToken ct)
    {
        return await repo.JournalTranscriptGetActiveByJournalEntryFk(journalEntryId, ct) is not null;
    }

    public async Task<OlieTranscribeResult> Transcribe(string localFile, CancellationToken ct)
    {
        var info = owr.GetOlieWavInfo(localFile);
        return await os.GoogleTranscribeWavNoEx(localFile, info, ct);
    }
}
