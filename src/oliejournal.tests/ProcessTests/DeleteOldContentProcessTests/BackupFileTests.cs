using oliejournal.lib.Processes.DeleteOldContentProcess;

namespace oliejournal.tests.ProcessTests.DeleteOldContentProcessTests;

public class BackupFileTests
{
    [Test]
    public void Effective_DefaultValue_BadFilename()
    {
        // Arrange
        var bf = new BackupFile
        {
            BackupFilePath = "badfilename.sql"
        };

        // Act
        var result = bf.Effective;

        // Assert
        Assert.That(result, Is.EqualTo(DateTime.MinValue));
    }
}
