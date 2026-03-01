using Azure.Storage.Blobs;
using Moq;
using oliejournal.data.Entities;
using oliejournal.lib.Processes.DeleteUserProcess;
using oliejournal.lib.Processes.JournalProcess;
using oliejournal.lib.Services;

namespace oliejournal.tests.ProcessTests.DeleteUserProcessTests;

public class DeleteUserProcessTests
{
    [Test]
    public async Task DeleteAllUserData_ShouldDeleteAllDataForUser()
    {
        // Arrange
        var entity = new UserDeleteLogEntity();
        var deleteUser = new Mock<IDeleteUserUnit>();
        deleteUser.Setup(s => s.CreateDeleteLog(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);
        var ingest = new Mock<IJournalEntryIngestionUnit>();
        ingest.Setup(s => s.GetJournalEntryList(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(
            [
                new JournalEntryEntity { Id = 1, UserId = "test-user-id" },
                new JournalEntryEntity { Id = 2, UserId = "test-user-id" }
            ]);
        var chatbot = new Mock<IJournalEntryChatbotUnit>();
        var voiceover = new Mock<IJournalEntryVoiceoverUnit>();
        var kinde = new Mock<IOlieKinde>();
        var process = new DeleteUserProcess(deleteUser.Object, ingest.Object, chatbot.Object, voiceover.Object, kinde.Object);
        string userId = "test-user-id";
        var blobContainerClient = new BlobContainerClient("UseDevelopmentStorage=true", "test-container");

        // Act
        await process.DeleteAllUserData(userId, blobContainerClient, CancellationToken.None);

        // Assert
        deleteUser.Verify(x => x.CreateDeleteLog(userId, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once);
        ingest.Verify(x => x.GetJournalEntryList(userId, It.IsAny<CancellationToken>()), Times.Once);
        chatbot.Verify(x => x.DeleteAllConversations(userId, It.IsAny<CancellationToken>()), Times.Once);
        ingest.Verify(x => x.DeleteVoice(It.IsAny<JournalEntryEntity>(), blobContainerClient, It.IsAny<CancellationToken>()), Times.AtLeastOnce);
        voiceover.Verify(x => x.DeleteLocalFile(It.IsAny<JournalEntryEntity>()), Times.AtLeastOnce);
        ingest.Verify(x => x.DeleteJournalEntry(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
        kinde.Verify(x => x.DeleteUser(userId, It.IsAny<CancellationToken>()), Times.Once);
        deleteUser.Verify(x => x.UpdateDeleteLog(It.IsAny<UserDeleteLogEntity>(), It.IsAny<CancellationToken>()), Times.Once);
        Assert.That(entity.Completed, Is.Not.Null);
        Assert.That(entity.Completed.Value, Is.InRange(DateTime.UtcNow.AddSeconds(-5), DateTime.UtcNow));
    }

    [Test]
    public async Task DeleteAllUserData_ShouldLogException()
    {
        // Arrange
        const string error = "Dillon Bang!";
        var entity = new UserDeleteLogEntity();
        var deleteUser = new Mock<IDeleteUserUnit>();
        deleteUser.Setup(s => s.CreateDeleteLog(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);
        var ingest = new Mock<IJournalEntryIngestionUnit>();
        ingest.Setup(s => s.GetJournalEntryList(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ApplicationException(error));
        var chatbot = new Mock<IJournalEntryChatbotUnit>();
        var voiceover = new Mock<IJournalEntryVoiceoverUnit>();
        var kinde = new Mock<IOlieKinde>();
        var process = new DeleteUserProcess(deleteUser.Object, ingest.Object, chatbot.Object, voiceover.Object, kinde.Object);
        string userId = "test-user-id";
        var blobContainerClient = new BlobContainerClient("UseDevelopmentStorage=true", "test-container");

        // Act
        await process.DeleteAllUserData(userId, blobContainerClient, CancellationToken.None);

        // Assert
        Assert.That(entity.Notes, Contains.Substring(error));
    }
}
