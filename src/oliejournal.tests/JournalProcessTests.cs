using Moq;
using oliejournal.data.Entities;
using oliejournal.lib;
using oliejournal.lib.Services.Models;
using oliejournal.lib.Units;

namespace oliejournal.tests;

public class JournalProcessTests
{
    private static (JournalProcess, Mock<IJournalEntryIngestionUnit>, Mock<IJournalEntryTranscribeUnit>, Mock<IJournalEntryChatbotUnit>) CreateUnit()
    {
        var ingestion = new Mock<IJournalEntryIngestionUnit>();
        var transcribe = new Mock<IJournalEntryTranscribeUnit>();
        var chatbot = new Mock<IJournalEntryChatbotUnit>();

        return (new JournalProcess(ingestion.Object, transcribe.Object, chatbot.Object), ingestion, transcribe, chatbot);
    }

    #region ChatbotAudioEntry

    [Test]
    public async Task ChatbotAudioEntry_SkipsToEnd_AlreadyChatbotted()
    {
        // Arrange
        const int journalEntryId = 42;
        const int transcriptId = 12;
        var (unit, ingestion, _, chatbot) = CreateUnit();
        chatbot.Setup(s => s.GetJournalTranscriptOrThrow(journalEntryId, CancellationToken.None))
            .ReturnsAsync(new JournalTranscriptEntity { Id = transcriptId});
        chatbot.Setup(s => s.IsAlreadyChatbotted(transcriptId, CancellationToken.None))
            .ReturnsAsync(true);

        // Act
        await unit.ChatbotAudioEntry(journalEntryId, null!, CancellationToken.None);

        // Assert
        ingestion.Verify(v => v.CreateJournalMessage(journalEntryId, lib.Enums.AudioProcessStepEnum.VoiceOver, null!, CancellationToken.None), Times.Once());
    }

    [Test]
    public async Task ChatbotAudioEntry_SkipsToEnd_EmptyTranscript()
    {
        // Arrange
        const int journalEntryId = 42;
        const int transcriptId = 12;
        var (unit, ingestion, _, chatbot) = CreateUnit();
        chatbot.Setup(s => s.GetJournalTranscriptOrThrow(journalEntryId, CancellationToken.None))
            .ReturnsAsync(new JournalTranscriptEntity { Id = transcriptId });
        chatbot.Setup(s => s.IsAlreadyChatbotted(transcriptId, CancellationToken.None))
            .ReturnsAsync(false);

        // Act
        await unit.ChatbotAudioEntry(journalEntryId, null!, CancellationToken.None);

        // Assert
        ingestion.Verify(v => v.CreateJournalMessage(journalEntryId, lib.Enums.AudioProcessStepEnum.VoiceOver, null!, CancellationToken.None), Times.Once());
    }

    [Test]
    public async Task ChatbotAudioEntry_ThrowsException_ApiError()
    {
        // Arrange
        const int journalEntryId = 42;
        const int transcriptId = 12;
        const string userId = "abc";
        const string conversationId = "bcd";
        const string message = "dillon";
        var (unit, ingestion, transcribe, chatbot) = CreateUnit();
        transcribe.Setup(s => s.GetJournalEntryOrThrow(journalEntryId, CancellationToken.None))
            .ReturnsAsync(new JournalEntryEntity { UserId = userId });
        chatbot.Setup(s => s.GetJournalTranscriptOrThrow(journalEntryId, CancellationToken.None))
            .ReturnsAsync(new JournalTranscriptEntity { Id = transcriptId, Transcript = message });
        chatbot.Setup(s => s.IsAlreadyChatbotted(transcriptId, CancellationToken.None))
            .ReturnsAsync(false);
        chatbot.Setup(s => s.GetConversation(userId, CancellationToken.None))
            .ReturnsAsync(new ConversationEntity { Id = conversationId });
        chatbot.Setup(s => s.Chatbot(userId, message, conversationId, CancellationToken.None))
            .ReturnsAsync(new OlieChatbotResult { Exception = new ApplicationException() });

        // Act, Assert
        Assert.ThrowsAsync<ApplicationException>(async () => await unit.ChatbotAudioEntry(journalEntryId, null!, CancellationToken.None));
    }

    [Test]
    public async Task ChatbotAudioEntry_CompletesToEnd_ApiSuccess()
    {
        // Arrange
        const int journalEntryId = 42;
        const int transcriptId = 12;
        const string userId = "abc";
        const string conversationId = "bcd";
        const string message = "dillon";
        var (unit, ingestion, transcribe, chatbot) = CreateUnit();
        transcribe.Setup(s => s.GetJournalEntryOrThrow(journalEntryId, CancellationToken.None))
            .ReturnsAsync(new JournalEntryEntity { UserId = userId });
        chatbot.Setup(s => s.GetJournalTranscriptOrThrow(journalEntryId, CancellationToken.None))
            .ReturnsAsync(new JournalTranscriptEntity { Id = transcriptId, Transcript = message });
        chatbot.Setup(s => s.IsAlreadyChatbotted(transcriptId, CancellationToken.None))
            .ReturnsAsync(false);
        chatbot.Setup(s => s.GetConversation(userId, CancellationToken.None))
            .ReturnsAsync(new ConversationEntity { Id = conversationId });
        chatbot.Setup(s => s.Chatbot(userId, message, conversationId, CancellationToken.None))
            .ReturnsAsync(new OlieChatbotResult());

        // Act
        await unit.ChatbotAudioEntry(journalEntryId, null!, CancellationToken.None);

        // Assert
        ingestion.Verify(v => v.CreateJournalMessage(journalEntryId, lib.Enums.AudioProcessStepEnum.VoiceOver, null!, CancellationToken.None), Times.Once());
    }

    #endregion

    #region IngestAudioEntry

    [Test]
    public async Task IngesAudioEntry_ReturnsId_Success()
    {
        // Arrange
        var (unit, ingest, _, _) = CreateUnit();
        ingest.Setup(s => s.CreateJournalEntry(string.Empty, It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<OlieWavInfo>(), CancellationToken.None))
            .ReturnsAsync(new JournalEntryEntity { Id = 123 });

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
        var (unit, _, transcribe, _) = CreateUnit();
        transcribe.Setup(s => s.GetJournalEntryOrThrow(123, CancellationToken.None))
            .ThrowsAsync(new ApplicationException());

        // Act, Assert
        Assert.ThrowsAsync<ApplicationException>(async () => await unit.TranscribeAudioEntry(123, null!, null!, CancellationToken.None));
    }

    [Test]
    public async Task TranscribeAudioEntry_SkipsToEnd_AlreadyProcessed()
    {
        // Arrange
        var (unit, ingestion, transcribe, _) = CreateUnit();
        transcribe.Setup(s => s.IsAlreadyTranscribed(123, CancellationToken.None))
            .ReturnsAsync(true);

        // Act
        await unit.TranscribeAudioEntry(123, null!, null!, CancellationToken.None);

        // Assert
        transcribe.Verify(v => v.Cleanup(It.IsAny<string>()), Times.Never());
    }

    [Test]
    public async Task TranscribeAudioEntry_Throws_ApiFailure()
    {
        // Arrange
        var (unit, _, transcribe, _) = CreateUnit();
        transcribe.Setup(s => s.GetJournalEntryOrThrow(123, CancellationToken.None))
            .ReturnsAsync(new JournalEntryEntity());
        transcribe.Setup(s => s.IsAlreadyTranscribed(123, CancellationToken.None))
            .ReturnsAsync(false);
        transcribe.Setup(s => s.Transcribe(It.IsAny<string>(), CancellationToken.None))
            .ReturnsAsync(new OlieTranscribeResult { Exception = new ApplicationException() });

        // Act, Assert
        Assert.ThrowsAsync<ApplicationException>(async () => await unit.TranscribeAudioEntry(123, null!, null!, CancellationToken.None));
    }

    [Test]
    public async Task TranscribeAudioEntry_CompletesAllSteps_FullyProcessed()
    {
        // Arrange
        var (unit, _, transcribe, _) = CreateUnit();
        transcribe.Setup(s => s.GetJournalEntryOrThrow(123, CancellationToken.None))
            .ReturnsAsync(new JournalEntryEntity());
        transcribe.Setup(s => s.IsAlreadyTranscribed(123, CancellationToken.None))
            .ReturnsAsync(false);
        transcribe.Setup(s => s.Transcribe(It.IsAny<string>(), CancellationToken.None))
            .ReturnsAsync(new OlieTranscribeResult());

        // Act
        await unit.TranscribeAudioEntry(123, null!, null!, CancellationToken.None);

        // Assert
        transcribe.Verify(v => v.Cleanup(It.IsAny<string>()), Times.Once());
    }

    #endregion
}
