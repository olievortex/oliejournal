using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;

namespace oliejournal.lib.Services;

public interface IOlieService
{
    #region Blob

    Task BlobUploadFile(BlobContainerClient client, string fileName, string localFileName, CancellationToken ct);

    #endregion

    #region File

    Task FileWriteAllBytes(string path, byte[] data, CancellationToken ct);


    #endregion

    #region ServiceBus

    Task ServiceBusSendJson(ServiceBusSender sender, object data, CancellationToken ct);

    #endregion

    #region Stream

    Task<byte[]> StreamToByteArray(Stream stream, CancellationToken ct);

    #endregion
}
