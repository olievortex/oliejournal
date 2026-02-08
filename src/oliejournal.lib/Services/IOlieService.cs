using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;
using oliejournal.lib.Services.Models;

namespace oliejournal.lib.Services;

public interface IOlieService
{
    #region Blob

    Task BlobDownloadFile(BlobContainerClient client, string fileName, string localFileName, CancellationToken ct);

    Task BlobUploadFile(BlobContainerClient client, string fileName, string localFileName, CancellationToken ct);

    #endregion

    #region File

    void FileDelete(string path);

    bool FileExists(string path);

    Task FileWriteAllBytes(string path, byte[] data, CancellationToken ct);

    #endregion

    #region Google

    Task<OlieTranscribeResult> GoogleTranscribeWavNoEx(string localFile, OlieWavInfo info, CancellationToken ct);

    #endregion

    #region ServiceBus

    Task ServiceBusSendJson(ServiceBusSender sender, object data, CancellationToken ct);

    Task ServiceBusCompleteMessage<T>(ServiceBusReceiver receiver, OlieServiceBusReceivedMessage<T> message, CancellationToken ct);

    Task<OlieServiceBusReceivedMessage<T>?> ServiceBusReceiveJson<T>(ServiceBusReceiver receiver, TimeSpan timeout, CancellationToken ct);

    #endregion

    #region Stream

    Task<byte[]> StreamToByteArray(Stream stream, CancellationToken ct);

    #endregion
}
