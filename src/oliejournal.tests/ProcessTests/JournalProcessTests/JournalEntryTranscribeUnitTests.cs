using Azure.Storage.Blobs;
using Moq;
using oliejournal.data;
using oliejournal.data.Entities;
using oliejournal.lib.Processes.JournalProcess;
using oliejournal.lib.Services;
using oliejournal.lib.Services.Models;
using System.Diagnostics;

namespace oliejournal.tests.ProcessTests.JournalProcessTests;

public class JournalEntryTranscribeUnitTests
{
    private static (JournalEntryTranscribeUnit, Mock<IOlieWavReader>, Mock<IOlieService>, Mock<IMyRepository>) CreateUnit()
    {
        var owr = new Mock<IOlieWavReader>();
        var os = new Mock<IOlieService>();
        var repo = new Mock<IMyRepository>();

        var unit = new JournalEntryTranscribeUnit(owr.Object, os.Object, repo.Object);

        return (unit, owr, os, repo);
    }

    #region Cleanup

    [Test]
    public void Cleanup_CallsFileDelete()
    {
        // Arrange
        var (unit, _, os, _) = CreateUnit();
        var path = "C:\\temp\\file.wav";

        // Act
        unit.Cleanup(path);

        // Assert
        os.Verify(x => x.FileDelete(path), Times.Once);
    }

    #endregion

    #region CreateTranscriptLog

    [Test]
    public async Task CreateTranscriptLog_TrimsLongFields_And_CallsRepo()
    {
        // Arrange
        const string exceptionMessage = "boom!";
        TranscriptLogEntity? captured = null;
        var (unit, _, os, repo) = CreateUnit();
        repo.Setup(r => r.TranscriptLogCreate(It.IsAny<TranscriptLogEntity>(), It.IsAny<CancellationToken>()))
             .Callback<TranscriptLogEntity, CancellationToken>((ent, ct) => { ent.Id = 123; captured = ent; })
             .Returns(Task.CompletedTask);

        var result = new OlieTranscribeResult
        {
            Cost = 42,
            Exception = new ApplicationException(exceptionMessage),
            ServiceId = 7
        };

        var sw = Stopwatch.StartNew();

        // Act
        await unit.CreateTranscriptLog(123, result, sw, CancellationToken.None);

        // Assert
        Assert.That(captured, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(captured.Id, Is.EqualTo(123));
            Assert.That(captured.ServiceId, Is.EqualTo(7));
            Assert.That(captured.Cost, Is.EqualTo(42));
            Assert.That(captured.ProcessingTime, Is.InRange(0, int.MaxValue));
            Assert.That(captured.Exception, Contains.Substring(exceptionMessage));
            Assert.That(captured.Created, Is.Not.EqualTo(DateTime.MinValue));
        }
    }

    [Test]
    public async Task CreateTranscriptLog_NoException_Nulls()
    {
        // Arrange
        TranscriptLogEntity? captured = null;
        var (unit, _, os, repo) = CreateUnit();
        repo.Setup(r => r.TranscriptLogCreate(It.IsAny<TranscriptLogEntity>(), It.IsAny<CancellationToken>()))
             .Callback<TranscriptLogEntity, CancellationToken>((ent, ct) => captured = ent)
             .Returns(Task.CompletedTask);

        var result = new OlieTranscribeResult
        {
            Cost = 42,
            ServiceId = 7
        };

        var sw = Stopwatch.StartNew();

        // Act
        await unit.CreateTranscriptLog(123, result, sw, CancellationToken.None);

        // Assert
        Assert.That(captured, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(captured.ServiceId, Is.EqualTo(7));
            Assert.That(captured.Cost, Is.EqualTo(42));
            Assert.That(captured.ProcessingTime, Is.InRange(0, int.MaxValue));
            Assert.That(captured.Exception, Is.Null);
            Assert.That(captured.Created, Is.Not.EqualTo(DateTime.MinValue));
        }
    }

    #endregion

    #region GetAudioFile

    [Test]
    public async Task GetAudioFile_WhenLocalExists_ReturnsLocalAndDoesNotDownload()
    {
        // Arrange
        var (unit, _, os, _) = CreateUnit();
        var blobPath = "folder/audio.wav";
        var expectedLocal = Path.GetTempPath() + Path.GetFileName(blobPath);

        os.Setup(o => o.FileExists(expectedLocal)).Returns(true);

        // Act
        var result = await unit.GetAudioFile(blobPath, null!, CancellationToken.None);

        // Assert
        Assert.That(result, Is.EqualTo(expectedLocal));
        os.Verify(o => o.BlobDownloadFile(It.IsAny<BlobContainerClient>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task GetAudioFile_WhenNotExists_DownloadsAndReturnsLocal()
    {
        // Arrange
        var (unit, _, os, _) = CreateUnit();
        var blobPath = "folder/audio2.wav";
        var expectedLocal = Path.GetTempPath() + Path.GetFileName(blobPath);

        os.Setup(o => o.FileExists(expectedLocal)).Returns(false);
        os.Setup(o => o.BlobDownloadFile(It.IsAny<BlobContainerClient>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
           .Returns(Task.CompletedTask);

        // Act
        var result = await unit.GetAudioFile(blobPath, null!, CancellationToken.None);

        // Assert
        Assert.That(result, Is.EqualTo(expectedLocal));
        os.Verify(o => o.BlobDownloadFile(It.IsAny<BlobContainerClient>(), blobPath, expectedLocal, It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region EnsureGoogleLimit

    [Test]
    public async Task EnsureGoogleLimit_WhenBillingUnderFree_DoesNotThrow()
    {
        // Arrange
        var (unit, _, _, repo) = CreateUnit();
        repo.Setup(r => r.TranscriptLogSummary(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(1000); // under free 3600

        // Act
        await unit.EnsureGoogleLimit(1, CancellationToken.None);

        // Assert
        Assert.Pass();
    }

    [Test]
    public async Task EnsureGoogleLimit_WhenCostExceedsLimit_Throws()
    {
        // Arrange
        var (unit, _, _, repo) = CreateUnit();

        // choose billing so (billing - 3600) * (0.016/60) > limit=1
        repo.Setup(r => r.TranscriptLogSummary(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(7360);

        // Act, Assert
        Assert.ThrowsAsync<ApplicationException>(() => unit.EnsureGoogleLimit(1, CancellationToken.None));
    }

    #endregion

    #region Transcribe

    [Test]
    public async Task Transcribe_CallsReaderAndService_ReturnsResult()
    {
        // Arrange
        var (unit, owr, os, _) = CreateUnit();
        var localFile = "somefile.wav";
        var info = new OlieWavInfo { Channels = 1, Duration = TimeSpan.FromSeconds(3) };

        owr.Setup(x => x.GetOlieWavInfo(localFile)).Returns(info);

        var expected = new OlieTranscribeResult { Transcript = "ok", Cost = 3, ServiceId = 2 };
        os.Setup(x => x.GoogleTranscribeWavNoEx(localFile, info, It.IsAny<CancellationToken>()))
           .ReturnsAsync(expected);

        // Act
        var result = await unit.Transcribe(localFile, CancellationToken.None);

        // Assert
        Assert.That(result, Is.EqualTo(expected));
        owr.Verify(x => x.GetOlieWavInfo(localFile), Times.Once);
        os.Verify(x => x.GoogleTranscribeWavNoEx(localFile, info, It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region UpdateEntry

    [Test]
    public async Task UpdateEntry_CallsRepo_VaidEntry()
    {
        // Arrange
        const string transcript = "Dill pickle";
        var (unit, _, _, repo) = CreateUnit();
        var entry = new JournalEntryEntity { Id = 123 };

        // Act
        await unit.UpdateEntry(transcript, entry, CancellationToken.None);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(entry.Transcript, Is.EqualTo(transcript));
            Assert.That(entry.TranscriptCreated, Is.Not.Null);
        }
    }

    #endregion
}