using Moq;
using oliejournal.data.Entities;
using oliejournal.lib.Processes.JournalProcess;
using oliejournal.lib.Services.Models;
using System.Diagnostics;

namespace oliejournal.tests.ProcessTests.JournalProcessTests;

public class JournalProcessTests
{
    private static (JournalProcess, Mock<IJournalEntryIngestionUnit>, Mock<IJournalEntryTranscribeUnit>, Mock<IJournalEntryChatbotUnit>, Mock<IJournalEntryVoiceoverUnit>) CreateUnit()
    {
        var ingestion = new Mock<IJournalEntryIngestionUnit>();
        var transcribe = new Mock<IJournalEntryTranscribeUnit>();
        var chatbot = new Mock<IJournalEntryChatbotUnit>();
        var voiceover = new Mock<IJournalEntryVoiceoverUnit>();

        return (new JournalProcess(ingestion.Object, transcribe.Object, chatbot.Object, voiceover.Object), ingestion, transcribe, chatbot, voiceover);
    }

    #region Voiceover

    [Test]
    public async Task Voiceover_ShortCircuits_AlreadyProcessed()
    {
        // Arrange
        const int journalEntryId = 42;
        var (unit, _, transcribe, _, voiceover) = CreateUnit();
        transcribe.Setup(s => s.GetJournalEntryOrThrow(journalEntryId, CancellationToken.None))
            .ReturnsAsync(new JournalEntryEntity { ResponsePath = "Dillon" });

        // Act
        await unit.Voiceover(journalEntryId, CancellationToken.None);

        // Assert
        voiceover.Verify(v => v.GetWavInfo(It.IsAny<byte[]>()), Times.Never());
    }

    [Test]
    public async Task Voiceover_ThrowsException_NoChatbotMessage()
    {
        // Arrange
        const int journalEntryId = 42;
        var (unit, _, transcribe, _, voiceover) = CreateUnit();
        transcribe.Setup(s => s.GetJournalEntryOrThrow(journalEntryId, CancellationToken.None))
            .ReturnsAsync(new JournalEntryEntity());
        voiceover.Setup(s => s.GetChatbotEntryOrThrow(journalEntryId, CancellationToken.None))
            .ReturnsAsync(new JournalChatbotEntity());

        // Act, Assert
        Assert.ThrowsAsync<ApplicationException>(async () => await unit.Voiceover(journalEntryId, CancellationToken.None));
    }

    [Test]
    public async Task Voiceover_UpdatesEntry_NotYetProcessed()
    {
        // Arrange
        const int journalEntryId = 42;
        const string message = "purple";
        const string blobPath = "green";
        var entry = new JournalEntryEntity();
        var file = "pastromi"u8.ToArray();
        var wavInfo = new OlieWavInfo();
        var stopwatch = Stopwatch.StartNew();
        var (unit, _, transcribe, _, voiceover) = CreateUnit();
        transcribe.Setup(s => s.GetJournalEntryOrThrow(journalEntryId, CancellationToken.None))
            .ReturnsAsync(entry);
        voiceover.Setup(s => s.GetChatbotEntryOrThrow(journalEntryId, CancellationToken.None))
            .ReturnsAsync(new JournalChatbotEntity { Message = message });
        voiceover.Setup(s => s.VoiceOver(message, CancellationToken.None))
            .ReturnsAsync(file);
        voiceover.Setup(s => s.SaveLocalFile(file, CancellationToken.None))
            .ReturnsAsync(blobPath);
        voiceover.Setup(s => s.GetWavInfo(file))
            .ReturnsAsync(wavInfo);

        // Act
        await unit.Voiceover(journalEntryId, CancellationToken.None);

        // Assert
        voiceover.Verify(v => v.UpdateEntry(blobPath, file.Length, It.IsAny<Stopwatch>(), wavInfo, entry, CancellationToken.None),
            Times.Once());
    }

    //public async Task Voiceover(int journalEntryId, CancellationToken ct)
    //{
    //    var entry = await transcribe.GetJournalEntryOrThrow(journalEntryId, ct);
    //    if (entry.ResponsePath != null) return;
    //    var chatbot = await voiceover.GetChatbotEntryOrThrow(journalEntryId, ct);
    //    if (chatbot.Message is null) throw new ApplicationException($"Chatbot response null for {journalEntryId}");

    //    var stopwatch = Stopwatch.StartNew();
    //    var file = await voiceover.VoiceOver(chatbot.Message, ct);
    //    var blobPath = await voiceover.SaveLocalFile(file, ct);
    //    var wavInfo = await voiceover.GetWavInfo(file);

    //    await voiceover.UpdateEntry(blobPath, file.Length, stopwatch, wavInfo, entry, ct);
    //}

    #endregion

    #region Chatbot

    [Test]
    public async Task Chatbot_SkipsToEnd_AlreadyChatbotted()
    {
        // Arrange
        const int journalEntryId = 42;
        const int transcriptId = 12;
        var (unit, ingestion, _, chatbot, _) = CreateUnit();
        chatbot.Setup(s => s.GetJournalTranscriptOrThrow(journalEntryId, CancellationToken.None))
            .ReturnsAsync(new JournalTranscriptEntity { Id = transcriptId });
        chatbot.Setup(s => s.IsAlreadyChatbotted(transcriptId, CancellationToken.None))
            .ReturnsAsync(true);

        // Act
        await unit.Chatbot(journalEntryId, null!, CancellationToken.None);

        // Assert
        ingestion.Verify(v => v.CreateJournalMessage(journalEntryId, lib.Enums.AudioProcessStepEnum.VoiceOver, null!, CancellationToken.None), Times.Once());
    }

    [Test]
    public async Task Chatbot_SkipsToEnd_EmptyTranscript()
    {
        // Arrange
        const int journalEntryId = 42;
        const int transcriptId = 12;
        var (unit, ingestion, _, chatbot, _) = CreateUnit();
        chatbot.Setup(s => s.GetJournalTranscriptOrThrow(journalEntryId, CancellationToken.None))
            .ReturnsAsync(new JournalTranscriptEntity { Id = transcriptId });
        chatbot.Setup(s => s.IsAlreadyChatbotted(transcriptId, CancellationToken.None))
            .ReturnsAsync(false);

        // Act
        await unit.Chatbot(journalEntryId, null!, CancellationToken.None);

        // Assert
        ingestion.Verify(v => v.CreateJournalMessage(journalEntryId, lib.Enums.AudioProcessStepEnum.VoiceOver, null!, CancellationToken.None), Times.Once());
    }

    [Test]
    public async Task Chatbot_ThrowsException_ApiError()
    {
        // Arrange
        const int journalEntryId = 42;
        const int transcriptId = 12;
        const string userId = "abc";
        const string conversationId = "bcd";
        const string message = "dillon";
        var (unit, ingestion, transcribe, chatbot, _) = CreateUnit();
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
        Assert.ThrowsAsync<ApplicationException>(async () => await unit.Chatbot(journalEntryId, null!, CancellationToken.None));
    }

    [Test]
    public async Task Chatbot_CompletesToEnd_ApiSuccess()
    {
        // Arrange
        const int journalEntryId = 42;
        const int transcriptId = 12;
        const string userId = "abc";
        const string conversationId = "bcd";
        const string message = "dillon";
        var (unit, ingestion, transcribe, chatbot, _) = CreateUnit();
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
        await unit.Chatbot(journalEntryId, null!, CancellationToken.None);

        // Assert
        ingestion.Verify(v => v.CreateJournalMessage(journalEntryId, lib.Enums.AudioProcessStepEnum.VoiceOver, null!, CancellationToken.None), Times.Once());
    }

    #endregion

    #region Ingest

    [Test]
    public async Task Ingest_ReturnsId_Success()
    {
        // Arrange
        var (unit, ingest, _, _, _) = CreateUnit();
        ingest.Setup(s => s.CreateJournalEntry(string.Empty, It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<OlieWavInfo>(), CancellationToken.None))
            .ReturnsAsync(new JournalEntryEntity { Id = 123 });

        // Act
        var result = await unit.Ingest(string.Empty, null!, null!, null!, CancellationToken.None);

        // Assert
        Assert.That(result, Is.EqualTo(123));
    }

    [Test]
    public async Task Ingest_ReturnsDuplicate_Duplicate()
    {
        // Arrange
        const string userId = "abc";
        const string hash = "dillon";
        var (unit, ingest, _, _, _) = CreateUnit();
        ingest.Setup(s => s.CreateHash(It.IsAny<byte[]>())).Returns(hash);
        ingest.Setup(s => s.GetDuplicateEntry(userId, hash, CancellationToken.None))
            .ReturnsAsync(new JournalEntryEntity { Id = 123 });

        // Act
        var result = await unit.Ingest(userId, null!, null!, null!, CancellationToken.None);

        // Assert
        Assert.That(result, Is.EqualTo(123));
    }

    #endregion

    #region Transcribe

    [Test]
    public async Task Transcribe_Throws_BadJournalEntryId()
    {
        // Arrange
        var (unit, _, transcribe, _, _) = CreateUnit();
        transcribe.Setup(s => s.GetJournalEntryOrThrow(123, CancellationToken.None))
            .ThrowsAsync(new ApplicationException());

        // Act, Assert
        Assert.ThrowsAsync<ApplicationException>(async () => await unit.Transcribe(123, null!, null!, CancellationToken.None));
    }

    [Test]
    public async Task Transcribe_SkipsToEnd_AlreadyProcessed()
    {
        // Arrange
        var (unit, ingestion, transcribe, _, _) = CreateUnit();
        transcribe.Setup(s => s.IsAlreadyTranscribed(123, CancellationToken.None))
            .ReturnsAsync(true);

        // Act
        await unit.Transcribe(123, null!, null!, CancellationToken.None);

        // Assert
        transcribe.Verify(v => v.Cleanup(It.IsAny<string>()), Times.Never());
    }

    [Test]
    public async Task Transcribe_Throws_ApiFailure()
    {
        // Arrange
        var (unit, _, transcribe, _, _) = CreateUnit();
        transcribe.Setup(s => s.GetJournalEntryOrThrow(123, CancellationToken.None))
            .ReturnsAsync(new JournalEntryEntity());
        transcribe.Setup(s => s.IsAlreadyTranscribed(123, CancellationToken.None))
            .ReturnsAsync(false);
        transcribe.Setup(s => s.Transcribe(It.IsAny<string>(), CancellationToken.None))
            .ReturnsAsync(new OlieTranscribeResult { Exception = new ApplicationException() });

        // Act, Assert
        Assert.ThrowsAsync<ApplicationException>(async () => await unit.Transcribe(123, null!, null!, CancellationToken.None));
    }

    [Test]
    public async Task Transcribe_CompletesAllSteps_FullyProcessed()
    {
        // Arrange
        var (unit, _, transcribe, _, _) = CreateUnit();
        transcribe.Setup(s => s.GetJournalEntryOrThrow(123, CancellationToken.None))
            .ReturnsAsync(new JournalEntryEntity());
        transcribe.Setup(s => s.IsAlreadyTranscribed(123, CancellationToken.None))
            .ReturnsAsync(false);
        transcribe.Setup(s => s.Transcribe(It.IsAny<string>(), CancellationToken.None))
            .ReturnsAsync(new OlieTranscribeResult());

        // Act
        await unit.Transcribe(123, null!, null!, CancellationToken.None);

        // Assert
        transcribe.Verify(v => v.Cleanup(It.IsAny<string>()), Times.Once());
    }

    #endregion
}
