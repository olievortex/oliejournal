using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Google.Cloud.Speech.V1;
using Newtonsoft.Json;
using oliejournal.lib.Services.Models;
using System.Diagnostics.CodeAnalysis;

namespace oliejournal.lib.Services;

[ExcludeFromCodeCoverage]
public class OlieService : IOlieService
{
    #region Blob

    public async Task BlobDownloadFile(BlobContainerClient client, string fileName, string localFileName, CancellationToken ct)
    {
        var blobClient = client.GetBlobClient(fileName);
        await blobClient.DownloadToAsync(localFileName, ct);
    }

    public async Task BlobUploadFile(BlobContainerClient client, string fileName, string localFileName, CancellationToken ct)
    {
        var blobClient = client.GetBlobClient(fileName);
        var contentType = "application/octet-stream";
        var extension = Path.GetExtension(fileName);

        if (extension.Equals(".gif", StringComparison.OrdinalIgnoreCase)) contentType = "image/gif";
        if (extension.Equals(".mp4", StringComparison.OrdinalIgnoreCase)) contentType = "video/mp4";
        if (extension.Equals(".wav", StringComparison.OrdinalIgnoreCase)) contentType = "audio/wav";

        var headers = new BlobHttpHeaders
        {
            CacheControl = "public, max-age=604800",
            ContentType = contentType
        };

        await blobClient.UploadAsync(localFileName, headers, cancellationToken: ct);
    }

    #endregion

    #region File

    public void FileDelete(string path)
    {
        File.Delete(path);
    }

    public bool FileExists(string path)
    {
        return File.Exists(path);
    }

    public async Task FileWriteAllBytes(string path, byte[] data, CancellationToken ct)
    {
        await File.WriteAllBytesAsync(path, data, ct);
    }

    #endregion

    #region Google

    public async Task<OlieTranscribeResult> GoogleTranscribeWav(string localFile, OlieWavInfo info, CancellationToken ct)
    {
        var transcript = string.Empty;

        if (info.Channels != 1) throw new ApplicationException("WAV must be mono");
        if (info.BitsPerSample != 16) throw new ApplicationException("WAV must be 16 bit");

        var client = await SpeechClient.CreateAsync(ct);
        var audio = await RecognitionAudio.FromFileAsync(localFile);
        var config = new RecognitionConfig
        {
            Encoding = RecognitionConfig.Types.AudioEncoding.Linear16,
            SampleRateHertz = info.SampleRate,
            LanguageCode = LanguageCodes.English.UnitedStates,
            EnableAutomaticPunctuation = true,
            EnableSpokenPunctuation = false
        };
        var response = await client.RecognizeAsync(config, audio, ct);

        foreach (var item in response.Results)
        {
            if (item.Alternatives.Count > 0)
            {
                var alternative = item.Alternatives[0];
                transcript = alternative.Transcript;
            }
        }

        return new OlieTranscribeResult
        {
            Transcript = transcript,
            Cost = (int)response.TotalBilledTime.Seconds
        };
    }


    #endregion

    #region ServiceBus

    public async Task ServiceBusSendJson(ServiceBusSender sender, object data, CancellationToken ct)
    {
        var json = JsonConvert.SerializeObject(data);
        var message = new ServiceBusMessage(json)
        {
            ContentType = "application/json"
        };

        await sender.SendMessageAsync(message, ct);
    }

    public async Task ServiceBusCompleteMessage<T>(ServiceBusReceiver receiver, OlieServiceBusReceivedMessage<T> message, CancellationToken ct)
    {
        await receiver.CompleteMessageAsync(message.ServiceBusReceivedMessage, ct);
    }

    public async Task<OlieServiceBusReceivedMessage<T>?> ServiceBusReceiveJson<T>(ServiceBusReceiver receiver, TimeSpan timeout, CancellationToken ct)
    {
        var message = await receiver.ReceiveMessageAsync(timeout, ct);
        if (message is null) return null;
        var json = message.Body.ToString();
        var body = JsonConvert.DeserializeObject<T>(json)
            ?? throw new InvalidCastException(json);

        return new OlieServiceBusReceivedMessage<T>
        {
            ServiceBusReceivedMessage = message,
            Body = body
        };
    }

    #endregion

    #region Stream

    public async Task<byte[]> StreamToByteArray(Stream stream, CancellationToken ct)
    {
        using var result = new MemoryStream();
        await stream.CopyToAsync(result, ct);

        return result.ToArray();
    }

    #endregion
}
