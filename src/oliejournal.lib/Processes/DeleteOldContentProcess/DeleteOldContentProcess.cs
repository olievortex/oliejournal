using Azure.Storage.Blobs;

namespace oliejournal.lib.Processes.DeleteOldContentProcess;

public class DeleteOldContentProcess(IMySqlMaintenance mysql) : IDeleteOldContentProcess
{
    public async Task Run(BlobContainerClient bcc, CancellationToken ct)
    {
        // Compress and upload MySQL Backups, delete after 7 days
        var backups = mysql.GetBackups();

        foreach (var backup in backups)
        {
            await mysql.StoreBackupOffsite(backup, bcc, ct);
            mysql.DeleteOldBackup(backup);
        }
    }
}