using Azure.Storage.Blobs;
using oliejournal.lib.Services;

namespace oliejournal.lib.Processes.DeleteOldContentProcess;

public class MySqlMaintenance(IOlieService os, IOlieConfig config) : IMySqlMaintenance
{
    const int BackupAgeDays = 7;

    public void DeleteOldBackup(BackupFile file)
    {
        if (file.Effective < DateTime.UtcNow.AddDays(-BackupAgeDays))
        {
            os.FileDeleteNoEx(file.BackupFilePath);
        }
    }

    public List<BackupFile> GetBackups()
    {
        var files = os.DirectoryList(config.MySqlBackupPath);
        var result = new List<BackupFile>();

        foreach (var file in files)
        {
            if (!(file.Contains("_olieblind_dev.sql") || file.Contains("_olieblind.sql"))) continue;

            result.Add(new BackupFile { BackupFilePath = file });
        }

        return [.. result.OrderByDescending(o => o.Effective)];
    }

    public async Task StoreBackupOffsite(BackupFile file, BlobContainerClient bcc, CancellationToken ct)
    {
        if (!file.IsCompressed)
        {
            var destination = Path.ChangeExtension(file.BackupFilePath, ".sql.gz");
            os.FileCompressGzip(file.BackupFilePath, destination);
            os.FileDelete(file.BackupFilePath);

            file.BackupFilePath = destination;
            await os.BlobUploadFile(bcc, Path.GetFileName(destination), destination, ct);
        }
    }
}
