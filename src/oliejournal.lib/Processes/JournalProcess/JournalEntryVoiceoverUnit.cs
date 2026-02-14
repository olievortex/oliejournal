using oliejournal.data;
using oliejournal.data.Entities;
using oliejournal.lib.Services;
using oliejournal.lib.Services.Models;
using System.Diagnostics;

namespace oliejournal.lib.Processes.JournalProcess;

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
        var blobWavPath = $"gold/audio_reply/{DateTime.UtcNow:yyyy/MM}/{filename}";
        var blobMp4Path = Path.ChangeExtension(blobWavPath, "mp4");
        var localWavPath = $"{config.GoldPath}/{blobWavPath}";
        var localMp4Path = $"{config.GoldPath}/{blobMp4Path}";

        os.FileCreateDirectory(localWavPath);
        await os.FileWriteAllBytes(localWavPath, bytes, ct);
        await os.FfmpegWavToMp3(localWavPath, localMp4Path, config.FfmpegPath, ct);
        os.FileDelete(localWavPath);

        return blobMp4Path;
    }

    public async Task UpdateEntry(string blobPath, int length, Stopwatch stopwatch, OlieWavInfo wavInfo, JournalEntryEntity entry, CancellationToken ct)
    {
        entry.ResponseCreated = DateTime.UtcNow;
        entry.ResponseDuration = (int)wavInfo.Duration.TotalSeconds;
        entry.ResponsePath = blobPath;
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
