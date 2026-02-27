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

    #region ChatbotLogCreate

    [Test]
    public async Task ChatbotLogCreate_CreatesRecord_WhenValid()
    {
        // Arrange
        const string exceptionMessage = "Bang!";
        ChatbotLogEntity? entity = null;
        var stopwatch = Stopwatch.StartNew();
        var chatbotResult = new OlieChatbotResult
        {
            ConversationId = "a",
            ServiceId = 234,
            InputTokens = 345,
            OutputTokens = 456,
            Exception = new ApplicationException(exceptionMessage),
            ResponseId = "b"
        };
        var (unit, repo, _, _) = CreateUnit();
        repo.Setup(s => s.ChatbotLogCreate(It.IsAny<ChatbotLogEntity>(), CancellationToken.None))
            .Callback<ChatbotLogEntity, CancellationToken>((e, _) => { e.Id = 567; entity = e; });

        // Act
        await unit.CreateChatbotLog(chatbotResult, stopwatch, CancellationToken.None);

        // Assert
        Assert.That(entity, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(entity.Id, Is.EqualTo(567));
            Assert.That(entity.ConversationId, Is.EqualTo("a"));
            Assert.That(entity.ProcessingTime, Is.InRange(0, 5));
            Assert.That(entity.InputTokens, Is.EqualTo(345));
            Assert.That(entity.OutputTokens, Is.EqualTo(456));
            Assert.That(entity.Exception, Contains.Substring(exceptionMessage));
            Assert.That(entity.Created, Is.Not.EqualTo(DateTime.MinValue));
            Assert.That(entity.ResponseId, Is.EqualTo("b"));
            Assert.That(entity.ServiceId, Is.EqualTo(234));
        }
    }

    [Test]
    public async Task ChatbotLogCreate_CreatesRecord_WhenNull()
    {
        // Arrange
        ChatbotLogEntity? entity = null;
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
        repo.Setup(s => s.ChatbotLogCreate(It.IsAny<ChatbotLogEntity>(), CancellationToken.None))
            .Callback<ChatbotLogEntity, CancellationToken>((e, _) => { e.Id = 567; entity = e; });

        // Act
        await unit.CreateChatbotLog(chatbotResult, stopwatch, CancellationToken.None);

        // Assert
        Assert.That(entity, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(entity.Id, Is.EqualTo(567));
            Assert.That(entity.ConversationId, Is.EqualTo("a"));
            Assert.That(entity.ProcessingTime, Is.InRange(0, 5));
            Assert.That(entity.InputTokens, Is.EqualTo(345));
            Assert.That(entity.OutputTokens, Is.EqualTo(456));
            Assert.That(entity.Exception, Is.Null);
            Assert.That(entity.Created, Is.Not.EqualTo(DateTime.MinValue));
            Assert.That(entity.ResponseId, Is.EqualTo("b"));
            Assert.That(entity.ServiceId, Is.EqualTo(234));
        }
    }

    #endregion

    #region DeleteAllConversations

    [Test]
    public async Task DeleteAllConversations_DeletesConversation_VeryOld()
    {
        // Arrange
        const string userId = "a";
        const string id = "42";
        var entity1 = new ChatbotConversationEntity
        {
            Id = id,
            Timestamp = DateTime.UtcNow.AddMonths(-1)
        };
        var entity2 = new ChatbotConversationEntity
        {
            Id = id,
            Timestamp = DateTime.UtcNow
        };
        var (unit, repo, _, _) = CreateUnit();
        repo.Setup(s => s.ChatbotConversationGetListByUser(userId, CancellationToken.None))
            .ReturnsAsync([entity1, entity2]);

        // Act
        await unit.DeleteAllConversations(userId, CancellationToken.None);

        // Assert
        repo.Verify(v => v.ChatbotConversationDelete(id, CancellationToken.None), Times.Exactly(2));
    }

    #endregion


    #region DeleteOldConversations

    [Test]
    public async Task DeleteOldConversations_DeletesConversation_VeryOld()
    {
        // Arrange
        const string userId = "a";
        const string id = "42";
        var entity = new ChatbotConversationEntity
        {
            Id = id,
            Timestamp = DateTime.UtcNow.AddMonths(-1)
        };
        var (unit, repo, _, _) = CreateUnit();
        repo.Setup(s => s.ChatbotConversationGetListByUser(userId, CancellationToken.None))
            .ReturnsAsync([entity]);

        // Act
        await unit.DeleteOldConversations(userId, CancellationToken.None);

        // Assert
        repo.Verify(v => v.ChatbotConversationDelete(id, CancellationToken.None), Times.Once);
    }

    [Test]
    public async Task DeleteOldConversations_KeepsConversation_VeryFresh()
    {
        // Arrange
        const string userId = "a";
        const string id = "42";
        var entity = new ChatbotConversationEntity
        {
            Id = id,
            Timestamp = DateTime.UtcNow
        };
        var (unit, repo, _, _) = CreateUnit();
        repo.Setup(s => s.ChatbotConversationGetListByUser(userId, CancellationToken.None))
            .ReturnsAsync([entity]);

        // Act
        await unit.DeleteOldConversations(userId, CancellationToken.None);

        // Assert
        repo.Verify(v => v.ChatbotConversationDelete(id, CancellationToken.None), Times.Never);
    }

    #endregion

    #region GetConversation

    [Test]
    public async Task GetConversation_ReturnsExisting_AlreadyExists()
    {
        // Arrange
        const string userId = "a";
        var conversation = new ChatbotConversationEntity();
        var (unit, repo, _, _) = CreateUnit();
        repo.Setup(s => s.ChatbotConversationGetListByUser(userId, CancellationToken.None))
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
        repo.Setup(s => s.ChatbotConversationGetListByUser(userId, CancellationToken.None))
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

    #region EnsureOpenAiLimit

    [Test]
    public async Task EnsureOpenAiLimit_NoException_BelowLimit()
    {
        // Arrange
        const int limit = 12;
        var (unit, repo, _, _) = CreateUnit();
        repo.Setup(s => s.ChatbotLogSummary(It.IsAny<DateTime>(), CancellationToken.None))
            .ReturnsAsync(new data.Models.ChatbotLogSummaryModel { InputTokens = 100, OutputTokens = 200 });

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
        repo.Setup(s => s.ChatbotLogSummary(It.IsAny<DateTime>(), CancellationToken.None))
            .ReturnsAsync(new data.Models.ChatbotLogSummaryModel { InputTokens = 10_000_000, OutputTokens = 20_000_000 });

        // Act, Assert
        Assert.ThrowsAsync<ApplicationException>(async () => await unit.EnsureOpenAiLimit(limit, CancellationToken.None));
    }

    #endregion

    #region UpdateEntry

    [Test]
    public async Task UpdateEntity_CallsRepo_ValidData()
    {
        // Arrange
        var (unit, repo, _, _) = CreateUnit();
        var message = new string('a', 9000);
        var entity = new JournalEntryEntity();

        // Act
        await unit.UpdateEntry(message, entity, CancellationToken.None);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(entity.Response, Is.EqualTo(message));
            Assert.That(entity.ResponseCreated, Is.Not.Null);
        }
        repo.Verify(s => s.JournalEntryUpdate(It.IsAny<JournalEntryEntity>(), CancellationToken.None), Times.Once);
    }

    #endregion
}
