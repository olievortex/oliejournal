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
        var (unit, ingest, _, _, voiceover) = CreateUnit();
        ingest.Setup(s => s.GetJournalEntryOrThrow(journalEntryId, CancellationToken.None))
            .ReturnsAsync(new JournalEntryEntity { VoiceoverPath = "Dillon" });

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
        var (unit, ingest, _, _, _) = CreateUnit();
        ingest.Setup(s => s.GetJournalEntryOrThrow(journalEntryId, CancellationToken.None))
            .ReturnsAsync(new JournalEntryEntity());

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
        var entry = new JournalEntryEntity { Response = message };
        var file = "pastromi"u8.ToArray();
        var wavInfo = new OlieWavInfo();
        var stopwatch = Stopwatch.StartNew();
        var (unit, ingest, _, _, voiceover) = CreateUnit();
        ingest.Setup(s => s.GetJournalEntryOrThrow(journalEntryId, CancellationToken.None))
            .ReturnsAsync(entry);
        voiceover.Setup(s => s.VoiceOver(message, CancellationToken.None))
            .ReturnsAsync(file);
        voiceover.Setup(s => s.SaveLocalFile(file, CancellationToken.None))
            .ReturnsAsync(blobPath);
        voiceover.Setup(s => s.GetWavInfo(file))
            .ReturnsAsync(wavInfo);

        // Act
        await unit.Voiceover(journalEntryId, CancellationToken.None);

        // Assert
        voiceover.Verify(v => v.UpdateEntry(blobPath, file.Length, wavInfo, entry, CancellationToken.None),
            Times.Once());
    }

    #endregion

    #region Chatbot

    [Test]
    public async Task Chatbot_SkipsToEnd_AlreadyChatbotted()
    {
        // Arrange
        const int journalEntryId = 42;
        const string response = "dillon!";
        var (unit, ingest, _, chatbot, _) = CreateUnit();
        ingest.Setup(s => s.GetJournalEntryOrThrow(journalEntryId, CancellationToken.None))
            .ReturnsAsync(new JournalEntryEntity { Response = response });

        // Act
        await unit.Chatbot(journalEntryId, null!, CancellationToken.None);

        // Assert
        chatbot.Verify(v => v.UpdateEntry(It.IsAny<string>(), It.IsAny<JournalEntryEntity>(), CancellationToken.None), Times.Never());
    }

    [Test]
    public async Task Chatbot_ThrowsException_EmptyTranscript()
    {
        // Arrange
        const int journalEntryId = 42;
        var (unit, ingest, _, _, _) = CreateUnit();
        ingest.Setup(s => s.GetJournalEntryOrThrow(journalEntryId, CancellationToken.None))
            .ReturnsAsync(new JournalEntryEntity());

        // Act, Assert
        Assert.ThrowsAsync<ApplicationException>(async () => await unit.Chatbot(journalEntryId, null!, CancellationToken.None));
    }

    [Test]
    public async Task Chatbot_ThrowsException_ApiError()
    {
        // Arrange
        const int journalEntryId = 42;
        const string userId = "abc";
        const string conversationId = "bcd";
        const string message = "dillon";
        var (unit, ingest, _, chatbot, _) = CreateUnit();
        ingest.Setup(s => s.GetJournalEntryOrThrow(journalEntryId, CancellationToken.None))
            .ReturnsAsync(new JournalEntryEntity { UserId = userId, Transcript = message });
        chatbot.Setup(s => s.GetConversation(userId, CancellationToken.None))
            .ReturnsAsync(new ChatbotConversationEntity { Id = conversationId });
        chatbot.Setup(s => s.Chatbot(userId, message, conversationId, CancellationToken.None))
            .ReturnsAsync(new OlieChatbotResult { Exception = new ApplicationException() });

        // Act, Assert
        Assert.ThrowsAsync<ApplicationException>(async () => await unit.Chatbot(journalEntryId, null!, CancellationToken.None));
    }

    [Test]
    public async Task Chatbot_ThrowsException_ApiReturnsNull()
    {
        // Arrange
        const int journalEntryId = 42;
        const string userId = "abc";
        const string conversationId = "bcd";
        const string message = "dillon";
        var (unit, ingest, _, chatbot, _) = CreateUnit();
        ingest.Setup(s => s.GetJournalEntryOrThrow(journalEntryId, CancellationToken.None))
            .ReturnsAsync(new JournalEntryEntity { UserId = userId, Transcript = message });
        chatbot.Setup(s => s.GetConversation(userId, CancellationToken.None))
            .ReturnsAsync(new ChatbotConversationEntity { Id = conversationId });
        chatbot.Setup(s => s.Chatbot(userId, message, conversationId, CancellationToken.None))
            .ReturnsAsync(new OlieChatbotResult());

        // Act, Assert
        Assert.ThrowsAsync<ApplicationException>(async () => await unit.Chatbot(journalEntryId, null!, CancellationToken.None));
    }

    [Test]
    public async Task Chatbot_CompletesToEnd_ApiSuccess()
    {
        // Arrange
        const int journalEntryId = 42;
        const string userId = "abc";
        const string conversationId = "bcd";
        const string message = "dillon";
        const string response = "pastromi";
        var (unit, ingest, _, chatbot, _) = CreateUnit();
        ingest.Setup(s => s.GetJournalEntryOrThrow(journalEntryId, CancellationToken.None))
            .ReturnsAsync(new JournalEntryEntity { UserId = userId, Transcript = message });
        chatbot.Setup(s => s.GetConversation(userId, CancellationToken.None))
            .ReturnsAsync(new ChatbotConversationEntity { Id = conversationId });
        chatbot.Setup(s => s.Chatbot(userId, message, conversationId, CancellationToken.None))
            .ReturnsAsync(new OlieChatbotResult { Message = response });

        // Act
        await unit.Chatbot(journalEntryId, null!, CancellationToken.None);

        // Assert
        ingest.Verify(v => v.CreateJournalMessage(journalEntryId, lib.Enums.AudioProcessStepEnum.VoiceOver, null!, CancellationToken.None), Times.Once());
    }

    #endregion

    #region GetEntryList

    [Test]
    public async Task GetEntryList_ReturnsList_RecordsFound()
    {
        // Arrange
        const string userId = "abc";
        const int id = 42;
        const string responseTest = "bcd";
        const string responsePath = "cde";
        const string transcript = "def";
        const float lat = 45.123f;
        const float lon = -93.456f;
        var entities = new List<JournalEntryEntity> {
            new() {
                Id = id,
                UserId = userId,
                Response = responseTest,
                VoiceoverPath = responsePath,
                Transcript = transcript,
                Latitude = lat,
                Longitude = lon,
                Created = DateTime.UtcNow }
        };
        var (unit, ingest, _, _, _) = CreateUnit();
        ingest.Setup(s => s.GetJournalEntryList(userId, CancellationToken.None))
            .ReturnsAsync(entities);

        // Act
        var result = await unit.GetEntryList(userId, CancellationToken.None);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result[0].UserId, Is.EqualTo(userId));
            Assert.That(result[0].Id, Is.EqualTo(id));
            Assert.That(result[0].ResponseText, Is.EqualTo(responseTest));
            Assert.That(result[0].ResponsePath, Is.EqualTo(responsePath));
            Assert.That(result[0].Transcript, Is.EqualTo(transcript));
            Assert.That(result[0].Created, Is.Not.EqualTo(DateTime.MinValue));
            Assert.That(result[0].Latitude, Is.EqualTo(lat).Within(0.0001));
            Assert.That(result[0].Longitude, Is.EqualTo(lon).Within(0.0001));
        }
    }

    #endregion

    #region GetEntry

    [Test]
    public async Task GetEntry_ReturnsNull_RecordNotFound()
    {
        // Arrange
        const int journalEntryId = 42;
        const string userId = "abc";
        var (unit, _, _, _, _) = CreateUnit();

        // Act
        var result = await unit.GetEntry(journalEntryId, userId, CancellationToken.None);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetEntry_ReturnsRecord_RecordFound()
    {
        // Arrange
        const int journalEntryId = 42;
        const string userId = "abc";
        var entity = new JournalEntryEntity();
        var (unit, ingest, _, _, _) = CreateUnit();
        ingest.Setup(s => s.GetJournalEntry(journalEntryId, userId, CancellationToken.None))
            .ReturnsAsync(entity);

        // Act
        var result = await unit.GetEntry(journalEntryId, userId, CancellationToken.None);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(entity.Id));
    }

    #endregion

    #region Ingest

    [Test]
    public async Task Ingest_ReturnsId_Success()
    {
        // Arrange
        var (unit, ingest, _, _, _) = CreateUnit();
        ingest.Setup(s => s.CreateJournalEntry(string.Empty, It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(),
            It.IsAny<float?>(), It.IsAny<float?>(), It.IsAny<OlieWavInfo>(), CancellationToken.None))
            .ReturnsAsync(new JournalEntryEntity { Id = 123 });

        // Act
        var result = await unit.Ingest(string.Empty, null!, null, null, null!, null!, CancellationToken.None);

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
        var result = await unit.Ingest(userId, null!, null, null, null!, null!, CancellationToken.None);

        // Assert
        Assert.That(result, Is.EqualTo(123));
    }

    #endregion

    #region Transcribe

    [Test]
    public async Task Transcribe_Throws_BadJournalEntryId()
    {
        // Arrange
        var (unit, ingest, _, _, _) = CreateUnit();
        ingest.Setup(s => s.GetJournalEntryOrThrow(123, CancellationToken.None))
            .ThrowsAsync(new ApplicationException());

        // Act, Assert
        Assert.ThrowsAsync<ApplicationException>(async () => await unit.Transcribe(123, null!, null!, CancellationToken.None));
    }

    [Test]
    public async Task Transcribe_SkipsToEnd_AlreadyProcessed()
    {
        // Arrange
        const string transcript = "dillon";
        var (unit, ingest, transcribe, _, _) = CreateUnit();
        ingest.Setup(s => s.GetJournalEntryOrThrow(123, CancellationToken.None))
            .ReturnsAsync(new JournalEntryEntity { Transcript = transcript });

        // Act
        await unit.Transcribe(123, null!, null!, CancellationToken.None);

        // Assert
        transcribe.Verify(v => v.Cleanup(It.IsAny<string>()), Times.Never());
    }

    [Test]
    public async Task Transcribe_Throws_ApiFailure()
    {
        // Arrange
        var (unit, ingest, transcribe, _, _) = CreateUnit();
        ingest.Setup(s => s.GetJournalEntryOrThrow(123, CancellationToken.None))
            .ReturnsAsync(new JournalEntryEntity());
        transcribe.Setup(s => s.Transcribe(It.IsAny<string>(), CancellationToken.None))
            .ReturnsAsync(new OlieTranscribeResult { Exception = new ApplicationException() });

        // Act, Assert
        Assert.ThrowsAsync<ApplicationException>(async () => await unit.Transcribe(123, null!, null!, CancellationToken.None));
    }

    [Test]
    public async Task Transcribe_Throws_NullApiResponse()
    {
        // Arrange
        var (unit, ingest, transcribe, _, _) = CreateUnit();
        ingest.Setup(s => s.GetJournalEntryOrThrow(123, CancellationToken.None))
            .ReturnsAsync(new JournalEntryEntity());
        transcribe.Setup(s => s.Transcribe(It.IsAny<string>(), CancellationToken.None))
            .ReturnsAsync(new OlieTranscribeResult());

        // Act, Assert
        Assert.ThrowsAsync<ApplicationException>(async () => await unit.Transcribe(123, null!, null!, CancellationToken.None));
    }

    [Test]
    public async Task Transcribe_CompletesAllSteps_FullyProcessed()
    {
        // Arrange
        const string transcript = "peggy";
        var (unit, ingest, transcribe, _, _) = CreateUnit();
        ingest.Setup(s => s.GetJournalEntryOrThrow(123, CancellationToken.None))
            .ReturnsAsync(new JournalEntryEntity());
        transcribe.Setup(s => s.Transcribe(It.IsAny<string>(), CancellationToken.None))
            .ReturnsAsync(new OlieTranscribeResult { Transcript = transcript });

        // Act
        await unit.Transcribe(123, null!, null!, CancellationToken.None);

        // Assert
        transcribe.Verify(v => v.Cleanup(It.IsAny<string>()), Times.Once());
    }

    #endregion

    #region DeleteEntry

    [Test]
    public async Task DeleteEntry_ReturnsFalse_AlreadyDeleted()
    {
        // Arrange
        const int journalEntryId = 42;
        const string userId = "abc";
        var (unit, _, _, _, _) = CreateUnit();

        // Act
        var result = await unit.DeleteEntry(journalEntryId, userId, null!, CancellationToken.None);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public async Task DeleteEntry_ReturnsTrue_Exists()
    {
        // Arrange
        const int journalEntryId = 42;
        const string userId = "abc";
        var (unit, ingest, _, _, _) = CreateUnit();
        ingest.Setup(s => s.GetJournalEntry(journalEntryId, userId, CancellationToken.None))
            .ReturnsAsync(new JournalEntryEntity { Id = journalEntryId });

        // Act
        var result = await unit.DeleteEntry(journalEntryId, userId, null!, CancellationToken.None);

        // Assert
        Assert.That(result, Is.True);
    }

    #endregion
}
