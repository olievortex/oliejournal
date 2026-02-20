using Azure.Storage.Blobs;
using Moq;
using oliejournal.lib.Processes.DeleteOldContentProcess;

namespace oliejournal.tests.ProcessTests.DeleteOldContentProcessTests;

public class DeleteOldContentProcessTests
{
    #region Run

    [Test]
    public async Task Run_LoopsOverItems_ItemsAvailable()
    {
        // Arrange
        var mysql = new Mock<IMySqlMaintenance>();
        var bcc = new Mock<BlobContainerClient>();
        var ct = CancellationToken.None;
        var backupFiles = new List<BackupFile>
        {
            new() { BackupFilePath = "/var/backups/mysql/20230101_010000_oliejournal_dev.sql" },
            new() { BackupFilePath = "/var/backups/mysql/20230201_010000_oliejournal.sql" }
        };
        mysql.Setup(m => m.GetBackups()).Returns(backupFiles);
        var process = new DeleteOldContentProcess(mysql.Object);

        // Act
        await process.Run(bcc.Object, ct);

        // Assert
        mysql.Verify(m => m.StoreBackupOffsite(backupFiles[0], bcc.Object, ct), Times.Once);
        mysql.Verify(m => m.StoreBackupOffsite(backupFiles[1], bcc.Object, ct), Times.Once);
        mysql.Verify(m => m.DeleteOldBackup(backupFiles[0]), Times.Once);
        mysql.Verify(m => m.DeleteOldBackup(backupFiles[1]), Times.Once);
    }

    #endregion
}
