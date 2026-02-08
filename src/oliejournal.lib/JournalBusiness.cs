using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;
using oliejournal.data;
using oliejournal.data.Entities;
using oliejournal.lib.Enums;
using oliejournal.lib.Models;
using oliejournal.lib.Services;

namespace oliejournal.lib;

public class JournalBusiness(IOlieWavReader owr, IMyRepository repo, IOlieService os) : IJournalBusiness
{
    public async Task<JournalEntryEntity> CreateJournalEntry(string userId, OlieWavInfo olieWavInfo, string path, int length, CancellationToken ct)
    {
        var entity = new JournalEntryEntity
        {
            UserId = userId,
            AudioBitsPerSample = olieWavInfo.BitsPerSample,
            AudioChannels = olieWavInfo.Channels,
            AudioDuration = (int)olieWavInfo.Duration.TotalSeconds,
            AudioLength = length,
            AudioSampleRate = olieWavInfo.SampleRate,
            Created = DateTime.UtcNow,
            AudioPath = path
        };

        await repo.JournalEntryCreate(entity, ct);

        return entity;
    }

    public async Task CreateJournalMessage(int id, AudioProcessStepEnum step, ServiceBusSender sender, CancellationToken ct)
    {
        var message = new AudioProcessQueueItemModel
        {
            Id = id,
            Step = step
        };

        await os.ServiceBusSendJson(sender, message, ct);
    }

    public OlieWavInfo EnsureAudioValidates(byte[] file)
    {
        if (file.Length == 0) throw new ApplicationException("WAV file empty");
        if (file.Length > 9 * 1024 * 1024) throw new ApplicationException($"WAV file {file.Length} > 9MB");

        var info = owr.GetOlieWavInfo(file);

        if (info.Channels > 1) throw new ApplicationException($"WAV file has {info.Channels} channels");
        if (info.SampleRate < 8000 || info.SampleRate > 48000) throw new ApplicationException($"WAV file has {info.SampleRate} sample rate");
        if (info.BitsPerSample != 16) throw new ApplicationException($"WAV file has {info.BitsPerSample} bits per sample");
        if (info.Duration > TimeSpan.FromSeconds(55)) throw new ApplicationException($"WAV file duration is {info.Duration.TotalSeconds}");

        return info;
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

    public async Task<string> GetAudioFile(string blobPath, BlobContainerClient client, CancellationToken ct)
    {
        var localFile = $"{Path.GetTempPath()}{Path.GetFileName(blobPath)}";
        if (os.FileExists(localFile)) return localFile;

        await os.BlobDownloadFile(client, blobPath, localFile, ct);
        return localFile;
    }

    public async Task<string> WriteAudioFileToBlob(string localPath, BlobContainerClient client, CancellationToken ct)
    {
        var blobPath = $"bronze/audio_entry/{DateTime.UtcNow:yyyy/MM}/{Path.GetFileName(localPath)}";
        await os.BlobUploadFile(client, blobPath, localPath, ct);

        return blobPath;
    }

    public async Task<string> WriteAudioFileToTemp(byte[] file, CancellationToken ct)
    {
        var localPath = $"{Path.GetTempPath()}{Guid.NewGuid()}.wav";
        await os.FileWriteAllBytes(localPath, file, ct);

        return localPath;
    }
}
