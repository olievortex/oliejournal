using Azure.Storage.Blobs;

namespace oliejournal.lib.Processes.DeleteUserProcess;

public interface IDeleteUserProcess
{
    Task DeleteAllUserData(string userId, BlobContainerClient client, CancellationToken ct);
}
