using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;
using oliejournal.data;
using oliejournal.lib.Enums;
using oliejournal.lib.Models;
using oliejournal.lib.Services;
using System.Diagnostics;

namespace oliejournal.lib;

public class JournalProcess(IJournalBusiness business, IOlieService os, IMyRepository repo, IOlieWavReader owr) : IJournalProcess
{
    public async Task<int> IngestAudioEntry(string userId, Stream audio, ServiceBusSender sender, BlobContainerClient client, CancellationToken ct)
    {
        var file = await os.StreamToByteArray(audio, ct);

        var wavInfo = business.EnsureAudioValidates(file);
        var localPath = await business.WriteAudioFileToTemp(file, ct);
        var blobPath = await business.WriteAudioFileToBlob(localPath, client, ct);
        var entry = await business.CreateJournalEntry(userId, wavInfo, blobPath, file.Length, ct);
        await business.CreateJournalMessage(entry.Id, AudioProcessStepEnum.Transcript, sender, ct);

        return entry.Id;
    }

    public async Task TranscribeAudioEntry(int id, BlobContainerClient client, CancellationToken ct)
    {
        var entity = await repo.JournalEntryGet(id, ct) ?? throw new ApplicationException($"Id {id} doesn't exist");
        if (entity.Transcript is not null) return;

        var localFile = await business.GetAudioFile(entity.AudioPath, client, ct);
        var stopwatch = Stopwatch.StartNew();

        var info = owr.GetOlieWavInfo(localFile);
        var transcript = await os.GoogleTranscribeWav(localFile, info, ct);

        entity.TranscriptProcessingTime = (int)stopwatch.Elapsed.TotalSeconds;
        entity.Transcript = transcript.Transcript;
        entity.TranscriptProcessingTime = transcript.Cost;

        await repo.JournalEntryUpdate(entity, ct);

        os.FileDelete(localFile);
        await os.ServiceBusSendJson(null!, new AudioProcessQueueItemModel { Id = id, Step = AudioProcessStepEnum.Chatbot }, ct);
    }
}
