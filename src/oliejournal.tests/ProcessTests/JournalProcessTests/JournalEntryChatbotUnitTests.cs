using Moq;
using oliejournal.data;
using oliejournal.data.Entities;
using oliejournal.lib.Processes.JournalProcess;
using oliejournal.lib.Services;
using oliejournal.lib.Services.Models;
using System.Diagnostics;

namespace oliejournal.tests.ProcessTests.JournalProcessTests;

public class JournalEntryChatbotUnitTests
{
    private static (JournalEntryChatbotUnit, Mock<IMyRepository>, Mock<IOlieService>, Mock<IOlieConfig>) CreateUnit()
    {
        var repo = new Mock<IMyRepository>();
        var os = new Mock<IOlieService>();
        var config = new Mock<IOlieConfig>();

        return (new JournalEntryChatbotUnit(repo.Object, os.Object, config.Object), repo, os, config);
    }

    #region Chatbot

    [Test]
    public async Task Chatbot_ReturnsMessage_WhenValid()
    {
        // Arrange
        var (unit, _, os, config) = CreateUnit();
        config.SetupGet(g => g.OpenAiModel).Returns("d");
        config.SetupGet(g => g.OpenAiApiKey).Returns("e");
        os.Setup(s => s.OpenAiEngageChatbotNoEx("a", "b", "c", "d", "e", CancellationToken.None))
            .ReturnsAsync(new OlieChatbotResult());

        // Act
        var response = await unit.Chatbot("a", "b", "c", CancellationToken.None);

        // Assert
        Assert.That(response, Is.Not.Null);
    }

    #endregion

    #region CreateJournalChatbot

    [Test]
    public async Task CreateJournalCatbot_CreatesRecord_WhenValid()
    {
        // Arrange
        JournalChatbotEntity? entity = null;
        const int journalTranscriptId = 123;
        var stopwatch = Stopwatch.StartNew();
        var chatbotResult = new OlieChatbotResult
        {
            ConversationId = "a",
            ServiceId = 234,
            Message = new string('a', 9000),
            InputTokens = 345,
            OutputTokens = 456,
            Exception = new ApplicationException(new string('b', 9000)),
            ResponseId = "b"
        };
        var (unit, repo, _, _) = CreateUnit();
        repo.Setup(s => s.JournalChatbotCreate(It.IsAny<JournalChatbotEntity>(), CancellationToken.None))
            .Callback<JournalChatbotEntity, CancellationToken>((e, _) => { e.Id = 567; entity = e; });

        // Act
        await unit.CreateJournalChatbot(journalTranscriptId, chatbotResult, stopwatch, CancellationToken.None);

        // Assert
        Assert.That(entity, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(entity.Id, Is.EqualTo(567));
            Assert.That(entity.ConversationFk, Is.EqualTo("a"));
            Assert.That(entity.JournalTranscriptFk, Is.EqualTo(journalTranscriptId));
            Assert.That(entity.ProcessingTime, Is.InRange(0, 5));
            Assert.That(entity.Message, Has.Length.EqualTo(8096));
            Assert.That(entity.Message, Contains.Substring("aaaa"));
            Assert.That(entity.InputTokens, Is.EqualTo(345));
            Assert.That(entity.OutputTokens, Is.EqualTo(456));
            Assert.That(entity.Exception, Has.Length.EqualTo(8096));
            Assert.That(entity.Exception, Contains.Substring("bbbb"));
            Assert.That(entity.Created, Is.Not.EqualTo(DateTime.MinValue));
            Assert.That(entity.ResponseId, Is.EqualTo("b"));
            Assert.That(entity.ServiceFk, Is.EqualTo(234));
        }
    }

    [Test]
    public async Task CreateJournalCatbot_CreatesRecord_WhenNull()
    {
        // Arrange
        JournalChatbotEntity? entity = null;
        const int journalTranscriptId = 123;
        var stopwatch = Stopwatch.StartNew();
        var chatbotResult = new OlieChatbotResult
        {
            ConversationId = "a",
            ServiceId = 234,
            InputTokens = 345,
            OutputTokens = 456,
            ResponseId = "b"
        };
        var (unit, repo, _, _) = CreateUnit();
        repo.Setup(s => s.JournalChatbotCreate(It.IsAny<JournalChatbotEntity>(), CancellationToken.None))
            .Callback<JournalChatbotEntity, CancellationToken>((e, _) => { e.Id = 567; entity = e; });

        // Act
        await unit.CreateJournalChatbot(journalTranscriptId, chatbotResult, stopwatch, CancellationToken.None);

        // Assert
        Assert.That(entity, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(entity.Id, Is.EqualTo(567));
            Assert.That(entity.ConversationFk, Is.EqualTo("a"));
            Assert.That(entity.JournalTranscriptFk, Is.EqualTo(journalTranscriptId));
            Assert.That(entity.ProcessingTime, Is.InRange(0, 5));
            Assert.That(entity.Message, Is.Null);
            Assert.That(entity.InputTokens, Is.EqualTo(345));
            Assert.That(entity.OutputTokens, Is.EqualTo(456));
            Assert.That(entity.Exception, Is.Null);
            Assert.That(entity.Created, Is.Not.EqualTo(DateTime.MinValue));
            Assert.That(entity.ResponseId, Is.EqualTo("b"));
            Assert.That(entity.ServiceFk, Is.EqualTo(234));
        }
    }

    #endregion

    #region DeleteConversations

    [Test]
    public async Task DeleteConversations_DeletesConversation_VeryOld()
    {
        // Arrange
        const string userId = "a";
        var entity = new ConversationEntity
        {
            Timestamp = DateTime.UtcNow.AddMonths(-1)
        };
        var (unit, repo, _, _) = CreateUnit();
        repo.Setup(s => s.ConversationGetActiveList(userId, CancellationToken.None))
            .ReturnsAsync([entity]);

        // Act
        await unit.DeleteConversations(userId, CancellationToken.None);

        // Assert
        Assert.That(entity.Deleted, Is.Not.Null);
    }

    [Test]
    public async Task DeleteConversations_KeepsConversation_VeryFresh()
    {
        // Arrange
        const string userId = "a";
        var entity = new ConversationEntity
        {
            Timestamp = DateTime.UtcNow
        };
        var (unit, repo, _, _) = CreateUnit();
        repo.Setup(s => s.ConversationGetActiveList(userId, CancellationToken.None))
            .ReturnsAsync([entity]);

        // Act
        await unit.DeleteConversations(userId, CancellationToken.None);

        // Assert
        Assert.That(entity.Deleted, Is.Null);
    }

    #endregion

    #region GetConversation

    [Test]
    public async Task GetConversation_ReturnsExisting_AlreadyExists()
    {
        // Arrange
        const string userId = "a";
        var conversation = new ConversationEntity();
        var (unit, repo, _, _) = CreateUnit();
        repo.Setup(s => s.ConversationGetActiveList(userId, CancellationToken.None))
            .ReturnsAsync([conversation]);

        // Act
        var result = await unit.GetConversation(userId, CancellationToken.None);

        // Assert
        Assert.That(result, Is.EqualTo(conversation));
    }

    [Test]
    public async Task GetConversation_ReturnsNew_DoesNotExist()
    {
        // Arrange
        const string userId = "a";
        var (unit, repo, os, config) = CreateUnit();
        config.SetupGet(g => g.ChatbotInstructions).Returns("a");
        config.SetupGet(g => g.OpenAiApiKey).Returns("b");
        repo.Setup(s => s.ConversationGetActiveList(userId, CancellationToken.None))
            .ReturnsAsync([]);
        os.Setup(s => s.OpenAiCreateConversation(userId, "a", "b", CancellationToken.None))
            .ReturnsAsync("c");

        // Act
        var result = await unit.GetConversation(userId, CancellationToken.None);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Id, Is.EqualTo("c"));
            Assert.That(result.UserId, Is.EqualTo(userId));
            Assert.That(result.Created, Is.Not.EqualTo(DateTime.MinValue));
            Assert.That(result.Timestamp, Is.Not.EqualTo(DateTime.MinValue));
        }
    }

    #endregion

    #region GetJournalTranscriptOrThrow

    [Test]
    public async Task GetJournalTranscriptOrThrow_Throws_NoTranscript()
    {
        // Arrange
        const int journalEntryId = 12;
        var (unit, _, _, _) = CreateUnit();

        // Act, Assert
        Assert.ThrowsAsync<ApplicationException>(async () => await unit.GetJournalTranscriptOrThrow(journalEntryId, CancellationToken.None));
    }

    [Test]
    public async Task GetJournalTranscriptOrThrow_ReturnsTranscript_TranscriptExists()
    {
        // Arrange
        const int journalEntryId = 12;
        var entity = new JournalTranscriptEntity();
        var (unit, repo, _, _) = CreateUnit();
        repo.Setup(s => s.JournalTranscriptGetByJournalEntryFk(journalEntryId, CancellationToken.None))
            .ReturnsAsync(entity);

        // Act
        var result = await unit.GetJournalTranscriptOrThrow(journalEntryId, CancellationToken.None);

        // Assert
        Assert.That(result, Is.EqualTo(entity));
    }

    #endregion

    #region EnsureOpenAiLimit

    [Test]
    public async Task EnsureOpenAiLimit_NoException_BelowLimit()
    {
        // Arrange
        const int limit = 12;
        var (unit, repo, _, _) = CreateUnit();
        repo.Setup(s => s.OpenApiGetChatbotSummary(It.IsAny<DateTime>(), CancellationToken.None))
            .ReturnsAsync(new data.Models.OpenAiCostSummary { InputTokens = 100, OutputTokens = 200 });

        // Act
        await unit.EnsureOpenAiLimit(limit, CancellationToken.None);

        // Assert
        Assert.Pass();
    }

    [Test]
    public async Task EnsureOpenAiLimit_Exception_AboveLimit()
    {
        // Arrange
        const int limit = 12;
        var (unit, repo, _, _) = CreateUnit();
        repo.Setup(s => s.OpenApiGetChatbotSummary(It.IsAny<DateTime>(), CancellationToken.None))
            .ReturnsAsync(new data.Models.OpenAiCostSummary { InputTokens = 10_000_000, OutputTokens = 20_000_000 });

        // Act, Assert
        Assert.ThrowsAsync<ApplicationException>(async () => await unit.EnsureOpenAiLimit(limit, CancellationToken.None));
    }

    #endregion

    #region IsAlreadyChatbotted

    [Test]
    public async Task IsAlreadyChatbotted_ReturnsFalse_NoResult()
    {
        // Arrange
        const int journalEntryId = 42;
        var (unit, _, _, _) = CreateUnit();

        // Act
        var result = await unit.IsAlreadyChatbotted(journalEntryId, CancellationToken.None);

        // Assert
        Assert.That(result, Is.False);
    }

    #endregion
}
