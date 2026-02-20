using Azure.Storage.Blobs;
using Moq;
using oliejournal.lib.Processes.DeleteOldContentProcess;
using oliejournal.lib.Services;

namespace oliejournal.tests.ProcessTests.DeleteOldContentProcessTests;


public class MySqlMaintenanceTests
{
    #region GetBackups

    [Test]
    public void GetBackups_ShouldReturnOrderedBackupFiles()
    {
        // Arrange
        var os = new Mock<IOlieService>();
        var config = new Mock<IOlieConfig>();
        config.Setup(c => c.MySqlBackupPath).Returns("/backups");
        var backupFiles = new List<string>
        {
            "/var/backups/mysql/20230101_010000_oliejournal_dev.sql",
            "/var/backups/mysql/20230201_010000_oliejournal.sql",
            "/var/backups/mysql/20230301_010000_oliejournal_dev.sql",
            "/var/backups/mysql/20230115_010000_oliejournal_dev.sql",
            "/var/backups/mysql/otherfile.txt"
        };
        os.Setup(s => s.DirectoryList("/backups")).Returns(backupFiles);
        var mySqlMaintenance = new MySqlMaintenance(os.Object, config.Object);

        // Act
        var result = mySqlMaintenance.GetBackups();

        // Assert
        Assert.That(result, Has.Count.EqualTo(4));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result[0].BackupFilePath, Is.EqualTo("/var/backups/mysql/20230301_010000_oliejournal_dev.sql"));
            Assert.That(result[1].BackupFilePath, Is.EqualTo("/var/backups/mysql/20230201_010000_oliejournal.sql"));
            Assert.That(result[2].BackupFilePath, Is.EqualTo("/var/backups/mysql/20230115_010000_oliejournal_dev.sql"));
            Assert.That(result[3].BackupFilePath, Is.EqualTo("/var/backups/mysql/20230101_010000_oliejournal_dev.sql"));
        }
    }

    #endregion

    #region StoreBackupOffsite

    /// <summary>
    /// Tests that StoreBackupOffsite does nothing when file is already compressed.
    /// Input: BackupFile with compressed path (.gz extension)
    /// Expected: No service methods are called
    /// </summary>
    [Test]
    public async Task StoreBackupOffsite_FileAlreadyCompressed_DoesNothing()
    {
        // Arrange
        var os = new Mock<IOlieService>();
        var config = new Mock<IOlieConfig>();
        var bcc = new Mock<BlobContainerClient>();
        var ct = CancellationToken.None;
        var file = new BackupFile { BackupFilePath = "/var/backups/mysql/20230101_010000_oliejournal.sql.gz" };
        var mySqlMaintenance = new MySqlMaintenance(os.Object, config.Object);

        // Act
        await mySqlMaintenance.StoreBackupOffsite(file, bcc.Object, ct);

        // Assert
        os.Verify(s => s.FileCompressGzip(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        os.Verify(s => s.FileDelete(It.IsAny<string>()), Times.Never);
        os.Verify(s => s.BlobUploadFile(It.IsAny<BlobContainerClient>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        Assert.That(file.BackupFilePath, Is.EqualTo("/var/backups/mysql/20230101_010000_oliejournal.sql.gz"));
    }

    /// <summary>
    /// Tests that StoreBackupOffsite correctly updates BackupFilePath property when compressing.
    /// Input: BackupFile with uncompressed path
    /// Expected: BackupFilePath is updated to compressed path with .sql.gz extension
    /// </summary>
    [Test]
    public async Task StoreBackupOffsite_FileNotCompressed_UpdatesBackupFilePath()
    {
        // Arrange
        var os = new Mock<IOlieService>();
        var config = new Mock<IOlieConfig>();
        var bcc = new Mock<BlobContainerClient>();
        var ct = CancellationToken.None;
        var file = new BackupFile { BackupFilePath = "/backups/database.sql" };
        var mySqlMaintenance = new MySqlMaintenance(os.Object, config.Object);

        // Act
        await mySqlMaintenance.StoreBackupOffsite(file, bcc.Object, ct);

        // Assert
        Assert.That(file.BackupFilePath, Is.EqualTo("/backups/database.sql.gz"));
    }

    #endregion

    #region DeleteOldBackup

    [Test]
    public void DeleteOldBackup_DeletesFile_WhenOld()
    {
        // Arrange
        const int backupAgeDays = 10;
        var filename = $"/var/backups/mysql/{DateTime.UtcNow.AddDays(-backupAgeDays):yyyyMMdd}_010000_oliejournal.sql.gz";
        var file = new BackupFile
        {
            BackupFilePath = filename,
        };
        var os = new Mock<IOlieService>();
        var unit = new MySqlMaintenance(os.Object, null!);

        // Act
        unit.DeleteOldBackup(file);

        // Assert
        os.Verify(v => v.FileDeleteNoEx(It.IsAny<string>()), Times.Once);
    }

    [Test]
    public void DeleteOldBackup_DoesNothing_WhenNotOld()
    {
        // Arrange
        var filename = $"/var/backups/mysql/{DateTime.UtcNow:yyyyMMdd}_010000_oliejournal.sql.gz";
        var file = new BackupFile
        {
            BackupFilePath = filename,
        };
        var os = new Mock<IOlieService>();
        var unit = new MySqlMaintenance(os.Object, null!);

        // Act
        unit.DeleteOldBackup(file);

        // Assert
        os.Verify(v => v.FileDeleteNoEx(It.IsAny<string>()), Times.Never);
    }

    #endregion
}