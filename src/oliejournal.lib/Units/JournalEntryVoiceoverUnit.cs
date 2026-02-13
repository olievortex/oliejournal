using oliejournal.data;
using oliejournal.data.Entities;
using oliejournal.lib.Services;
using oliejournal.lib.Services.Models;
using System.Diagnostics;

namespace oliejournal.lib.Units;

public class JournalEntryVoiceoverUnit(IMyRepository repo, IOlieService os, IOlieConfig config, IOlieWavReader owr) : IJournalEntryVoiceoverUnit
{
    public async Task<OlieWavInfo> GetWavInfo(byte[] bytes)
    {
        return owr.GetOlieWavInfo(bytes);
    }

    public async Task<JournalChatbotEntity> GetChatbotEntryOrThrow(int journalEntryId, CancellationToken ct)
    {
        return await repo.JournalChatbotGetByJournalEntryId(journalEntryId, ct) ??
            throw new ApplicationException($"JournalChatbot for journalEntryId {journalEntryId} doesn't exist");
    }

    public async Task<string> SaveLocalFile(byte[] bytes, CancellationToken ct)
    {
        var filename = $"{Guid.NewGuid()}.wav";
        var blobPath = $"gold/audio_reply/{DateTime.UtcNow:yyyy/MM}/{filename}";
        var localPath = $"{config.GoldPath}/{blobPath}";

        os.FileCreateDirectory(localPath);
        await os.FileWriteAllBytes(localPath, bytes, ct);

        return blobPath;
    }

    public async Task UpdateEntry(string localFilename, int length, Stopwatch stopwatch, OlieWavInfo wavInfo, JournalEntryEntity entry, CancellationToken ct)
    {
        entry.ResponseCreated = DateTime.UtcNow;
        entry.ResponseDuration = (int)wavInfo.Duration.TotalSeconds;
        entry.ResponsePath = localFilename;
        entry.ResponseProcessingTime = (int)stopwatch.Elapsed.TotalSeconds;
        entry.ResponseLength = length;

        await repo.JournalEntryUpdate(entry, ct);
    }

    public async Task<byte[]> VoiceOver(string script, CancellationToken ct)
    {
        var bytes = await os.GoogleSpeak(config.GoogleVoiceName, script, ct);

        return bytes;
    }
}
