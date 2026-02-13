using Moq;
using oliejournal.data;
using oliejournal.data.Entities;
using oliejournal.lib;

namespace oliejournal.tests;

public class JournalApiBusinessTests
{
    private static (JournalApiBusiness, Mock<IMyRepository>) CreateUnit()
    {
        var repo = new Mock<IMyRepository>();
        var unit = new JournalApiBusiness(repo.Object);

        return (unit, repo);
    }

    #region GetEntryList

    [Test]
    public async Task GetEntryList_ReturnsList_RecordsFound()
    {
        // Arrange
        const string userId = "abc";
        const int id = 42;
        const string responseTest = "bcd";
        var entities = new List<JournalEntryListEntity> {
            new() { Id = id, UserId = userId, ResponseText = responseTest, Created = DateTime.UtcNow } };
        var (unit, repo) = CreateUnit();
        repo.Setup(s => s.JournalEntryListGetByUserId(userId, CancellationToken.None))
            .ReturnsAsync(entities);

        // Act
        var result = await unit.GetEntryList(userId, CancellationToken.None);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Is.EqualTo(entities));
            Assert.That(result[0].UserId, Is.EqualTo(userId));
            Assert.That(result[0].Id, Is.EqualTo(id));
            Assert.That(result[0].ResponseText, Is.EqualTo(responseTest));
            Assert.That(result[0].Created, Is.Not.EqualTo(DateTime.MinValue));
        }
    }

    #endregion

    #region GetEntryStatus

    [Test]
    public async Task GetEntryStatus_Returns0_RecordNotFound()
    {
        // Arrange
        const int journalEntryId = 42;
        const string userId = "abc";
        var (unit, _) = CreateUnit();

        // Act
        var result = await unit.GetEntryStatus(journalEntryId, userId, CancellationToken.None);

        // Assert
        Assert.That(result, Is.Zero);
    }

    [Test]
    public async Task GetEntryStatus_Returns1_NoTranscript()
    {
        // Arrange
        const int journalEntryId = 42;
        const string userId = "abc";
        var entity = new JournalEntryListEntity();
        var (unit, repo) = CreateUnit();
        repo.Setup(s => s.JournalEntryListGetByUserId(journalEntryId, userId, CancellationToken.None))
            .ReturnsAsync(entity);

        // Act
        var result = await unit.GetEntryStatus(journalEntryId, userId, CancellationToken.None);

        // Assert
        Assert.That(result, Is.EqualTo(1));
    }

    [Test]
    public async Task GetEntryStatus_Returns2_NoReply()
    {
        // Arrange
        const int journalEntryId = 42;
        const string userId = "abc";
        var entity = new JournalEntryListEntity { Transcript = "Dillon" };
        var (unit, repo) = CreateUnit();
        repo.Setup(s => s.JournalEntryListGetByUserId(journalEntryId, userId, CancellationToken.None))
            .ReturnsAsync(entity);

        // Act
        var result = await unit.GetEntryStatus(journalEntryId, userId, CancellationToken.None);

        // Assert
        Assert.That(result, Is.EqualTo(2));
    }

    [Test]
    public async Task GetEntryStatus_Returns3_Complete()
    {
        // Arrange
        const int journalEntryId = 42;
        const string userId = "abc";
        var entity = new JournalEntryListEntity { Transcript = "Dillon", ResponsePath = "Silly" };
        var (unit, repo) = CreateUnit();
        repo.Setup(s => s.JournalEntryListGetByUserId(journalEntryId, userId, CancellationToken.None))
            .ReturnsAsync(entity);

        // Act
        var result = await unit.GetEntryStatus(journalEntryId, userId, CancellationToken.None);

        // Assert
        Assert.That(result, Is.EqualTo(3));
    }

    #endregion
}
