using Azure.Storage.Blobs;

namespace oliejournal.lib.Processes.DeleteUser;

public interface IDeleteUserProcess
{
    Task DeleteAllUserData(string userId, BlobContainerClient client, CancellationToken ct);
}
