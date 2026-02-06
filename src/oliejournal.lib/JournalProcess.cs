using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;
using oliejournal.lib.Enums;
using oliejournal.lib.Services;

namespace oliejournal.lib;

public class JournalProcess(IJournalBusiness business, IOlieService os) : IJournalProcess
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
}
