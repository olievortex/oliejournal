using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Newtonsoft.Json;
using System.Diagnostics.CodeAnalysis;

namespace oliejournal.lib.Services;

[ExcludeFromCodeCoverage]
public class OlieService : IOlieService
{
    #region Blob

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

    public async Task FileWriteAllBytes(string path, byte[] data, CancellationToken ct)
    {
        await File.WriteAllBytesAsync(path, data, ct);
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
