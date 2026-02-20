using Azure.Storage.Blobs;

namespace oliejournal.lib.Processes.DeleteOldContentProcess;

public interface IDeleteOldContentProcess
{
    Task Run(BlobContainerClient bcc, CancellationToken ct);
}
