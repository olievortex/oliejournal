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
        const string responsePath = "cde";
        const string transcript = "def";
        const float lat = 45.123f;
        const float lon = -93.456f;
        var entities = new List<JournalEntryListEntity> {
            new() {
                Id = id,
                UserId = userId,
                ResponseText = responseTest,
                ResponsePath = responsePath,
                Transcript = transcript,
                Latitude = lat,
                Longitude = lon,
                Created = DateTime.UtcNow }
        };
        var (unit, repo) = CreateUnit();
        repo.Setup(s => s.JournalEntryListGetByUserId(userId, CancellationToken.None))
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
        var (unit, _) = CreateUnit();

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
        var entity = new JournalEntryListEntity();
        var (unit, repo) = CreateUnit();
        repo.Setup(s => s.JournalEntryListGetByUserId(journalEntryId, userId, CancellationToken.None))
            .ReturnsAsync(entity);

        // Act
        var result = await unit.GetEntry(journalEntryId, userId, CancellationToken.None);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(entity.Id));
    }

    #endregion
}
