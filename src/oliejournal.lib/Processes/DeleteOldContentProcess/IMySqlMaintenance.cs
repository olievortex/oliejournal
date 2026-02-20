using Azure.Storage.Blobs;

namespace oliejournal.lib.Processes.DeleteOldContentProcess;

public interface IMySqlMaintenance
{
    void DeleteOldBackup(BackupFile file);
    List<BackupFile> GetBackups();
    Task StoreBackupOffsite(BackupFile file, BlobContainerClient bcc, CancellationToken ct);
}
