using Moq;
using oliejournal.data.Entities;
using oliejournal.lib;
using oliejournal.lib.Services.Models;
using oliejournal.lib.Units;

namespace oliejournal.tests;

public class JournalProcessTests
{
    #region IngestAudioEntry

    [Test]
    public async Task IngesAudioEntry_ReturnsId_Success()
    {
        // Arrange
        var ingest = new Mock<IJournalEntryIngestionUnit>();
        ingest.Setup(s => s.CreateJournalEntry(string.Empty, It.IsAny<OlieWavInfo>(), It.IsAny<string>(), It.IsAny<int>(), CancellationToken.None))
            .ReturnsAsync(new JournalEntryEntity { Id = 123 });
        var unit = new JournalProcess(ingest.Object, null!);

        // Act
        var result = await unit.IngestAudioEntry(string.Empty, null!, null!, null!, CancellationToken.None);

        // Assert
        Assert.That(result, Is.EqualTo(123));
    }

    #endregion

    #region TranscribeAudioEntry

    [Test]
    public async Task TranscribeAudioEntry_Throws_BadJournalEntryId()
    {
        // Arrange
        var transcribe = new Mock<IJournalEntryTranscribeUnit>();
        transcribe.Setup(s => s.GetJournalEntryOrThrow(123, CancellationToken.None))
            .ThrowsAsync(new ApplicationException());
        var unit = new JournalProcess(null!, transcribe.Object);

        // Act, Assert
        Assert.ThrowsAsync<ApplicationException>(async () => await unit.TranscribeAudioEntry(123, null!, null!, CancellationToken.None));
    }

    [Test]
    public async Task TranscribeAudioEntry_SkipsToEnd_AlreadyProcessed()
    {
        // Arrange
        var transcribe = new Mock<IJournalEntryTranscribeUnit>();
        var ingestion = new Mock<IJournalEntryIngestionUnit>();
        transcribe.Setup(s => s.IsAlreadyTranscribed(123, CancellationToken.None))
            .ReturnsAsync(true);
        var unit = new JournalProcess(ingestion.Object, transcribe.Object);

        // Act
        await unit.TranscribeAudioEntry(123, null!, null!, CancellationToken.None);

        // Assert
        transcribe.Verify(v => v.Cleanup(It.IsAny<string>()), Times.Never());
    }

    [Test]
    public async Task TranscribeAudioEntry_Throws_ApiFailure()
    {
        // Arrange
        var transcribe = new Mock<IJournalEntryTranscribeUnit>();
        var ingestion = new Mock<IJournalEntryIngestionUnit>();
        transcribe.Setup(s => s.GetJournalEntryOrThrow(123, CancellationToken.None))
            .ReturnsAsync(new JournalEntryEntity());
        transcribe.Setup(s => s.IsAlreadyTranscribed(123, CancellationToken.None))
            .ReturnsAsync(false);
        transcribe.Setup(s => s.Transcribe(It.IsAny<string>(), CancellationToken.None))
            .ReturnsAsync(new OlieTranscribeResult { Exception = new ApplicationException() });
        var unit = new JournalProcess(ingestion.Object, transcribe.Object);

        // Act, Assert
        Assert.ThrowsAsync<ApplicationException>(async () => await unit.TranscribeAudioEntry(123, null!, null!, CancellationToken.None));
    }

    [Test]
    public async Task TranscribeAudioEntry_CompletesAllSteps_FullyProcessed()
    {
        // Arrange
        var transcribe = new Mock<IJournalEntryTranscribeUnit>();
        var ingestion = new Mock<IJournalEntryIngestionUnit>();
        transcribe.Setup(s => s.GetJournalEntryOrThrow(123, CancellationToken.None))
            .ReturnsAsync(new JournalEntryEntity());
        transcribe.Setup(s => s.IsAlreadyTranscribed(123, CancellationToken.None))
            .ReturnsAsync(false);
        transcribe.Setup(s => s.Transcribe(It.IsAny<string>(), CancellationToken.None))
            .ReturnsAsync(new OlieTranscribeResult());
        var unit = new JournalProcess(ingestion.Object, transcribe.Object);

        // Act
        await unit.TranscribeAudioEntry(123, null!, null!, CancellationToken.None);

        // Assert
        transcribe.Verify(v => v.Cleanup(It.IsAny<string>()), Times.Once());
    }

    #endregion
}
