using Moq;
using oliejournal.data;
using oliejournal.data.Entities;
using oliejournal.lib.Processes.JournalProcess;
using oliejournal.lib.Services;
using oliejournal.lib.Services.Models;
using System.Diagnostics;

namespace oliejournal.tests.ProcessTests.JournalProcessTests;

public class JournalEntryVoiceoverUnitTests
{
    private static (JournalEntryVoiceoverUnit, Mock<IOlieWavReader>, Mock<IOlieService>, Mock<IMyRepository>, Mock<IOlieConfig>) CreateUnit()
    {
        var owr = new Mock<IOlieWavReader>();
        var os = new Mock<IOlieService>();
        var repo = new Mock<IMyRepository>();
        var config = new Mock<IOlieConfig>();

        var unit = new JournalEntryVoiceoverUnit(repo.Object, os.Object, config.Object, owr.Object);

        return (unit, owr, os, repo, config);
    }

    #region GetWavInfo

    [Test]
    public async Task GetWavInfo_ReturnsInfo_NotEmpty()
    {
        // Arrange
        var owi = new OlieWavInfo();
        var bytes = "Dillon"u8.ToArray();
        var (unit, owr, _, _, _) = CreateUnit();
        owr.Setup(s => s.GetOlieWavInfo(bytes))
            .Returns(owi);

        // Act
        var result = await unit.GetWavInfo(bytes);

        // Assert
        Assert.That(result, Is.EqualTo(owi));
    }

    #endregion

    #region GetChatbotEntryOrThrow

    [Test]
    public async Task GetChatbotEntryOrThrow_ReturnsValue_WhenFound()
    {
        // Arrange
        const int journalEntryId = 42;
        var entity = new JournalChatbotEntity();
        var (unit, _, _, repo, _) = CreateUnit();
        repo.Setup(s => s.JournalChatbotGetByJournalEntryId(journalEntryId, CancellationToken.None))
            .ReturnsAsync(entity);

        // Act
        var result = await unit.GetChatbotEntryOrThrow(journalEntryId, CancellationToken.None);

        // Assert
        Assert.That(result, Is.EqualTo(entity));
    }

    [Test]
    public async Task GetChatbotEntryOrThrow_ThrowsException_NotFound()
    {
        // Arrange
        const int journalEntryId = 42;
        var (unit, _, _, _, _) = CreateUnit();

        // Act, Assert
        Assert.ThrowsAsync<ApplicationException>(async () => await unit.GetChatbotEntryOrThrow(journalEntryId, CancellationToken.None));

    }

    #endregion

    #region SaveLocalFile

    [Test]
    public async Task SaveLocalFile_ReturnsBlobName_ValidBytes()
    {
        // Arrange
        var bytes = "Pastromi"u8.ToArray();
        var (unit, _, _, _, _) = CreateUnit();

        // Act
        var result = await unit.SaveLocalFile(bytes, CancellationToken.None);

        // Assert
        Assert.That(result, Contains.Substring(".mp4"));
    }

    #endregion

    #region UpdateEntry

    [Test]
    public async Task UpdateEntry_UpdatesEntity_WithParameters()
    {
        // Arrange
        var entry = new JournalEntryEntity();
        var wavInfo = new OlieWavInfo { Duration = TimeSpan.FromSeconds(1024) };
        var stopwatch = Stopwatch.StartNew();
        const int length = 42;
        const string blobName = "Pastromi";
        var (unit, _, _, _, _) = CreateUnit();

        // Act
        await unit.UpdateEntry(blobName, length, stopwatch, wavInfo, entry, CancellationToken.None);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(entry.ResponseCreated, Is.Not.Null);
            Assert.That(entry.ResponseDuration, Is.EqualTo(1024));
            Assert.That(entry.ResponsePath, Is.EqualTo(blobName));
            Assert.That(entry.ResponseProcessingTime, Is.Not.Null);
            Assert.That(entry.ResponseLength, Is.EqualTo(length));
        }
    }

    #endregion

    #region VoiceOver

    [Test]
    public async Task VoiceOver_ReturnsValue_ValidScript()
    {
        // Arrange
        const string script = "Dillon";
        const string voice = "Boo";
        var bytes = "Pastromi"u8.ToArray();
        var (unit, _, os, _, config) = CreateUnit();
        config.SetupGet(g => g.GoogleVoiceName).Returns(voice);
        os.Setup(s => s.GoogleSpeak(voice, script, CancellationToken.None))
            .ReturnsAsync(bytes);

        // Act
        var result = await unit.VoiceOver(script, CancellationToken.None);

        // Assert
        Assert.That(result, Is.EqualTo(bytes));
    }

    #endregion
}
