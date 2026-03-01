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
        var deleteUser = new Mock<IDeleteUserUnit>();
        var ingestiob = new Mock<IJournalEntryIngestionUnit>();
        ingestiob.Setup(s => s.GetJournalEntryList(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(
            [
                new JournalEntryEntity { Id = 1, UserId = "test-user-id" },
                new JournalEntryEntity { Id = 2, UserId = "test-user-id" }
            ]);
        var chatbot = new Mock<IJournalEntryChatbotUnit>();
        var voiceover = new Mock<IJournalEntryVoiceoverUnit>();
        var kinde = new Mock<IOlieKinde>();
        var process = new DeleteUserProcess(deleteUser.Object, ingestiob.Object, chatbot.Object, voiceover.Object, kinde.Object);
        string userId = "test-user-id";
        var blobContainerClient = new BlobContainerClient("UseDevelopmentStorage=true", "test-container");

        // Act
        await process.DeleteAllUserData(userId, blobContainerClient, CancellationToken.None);

        // Assert
        deleteUser.Verify(x => x.CreateDeleteLog(userId, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once);
        ingestiob.Verify(x => x.GetJournalEntryList(userId, It.IsAny<CancellationToken>()), Times.Once);
        chatbot.Verify(x => x.DeleteAllConversations(userId, It.IsAny<CancellationToken>()), Times.Once);
        ingestiob.Verify(x => x.DeleteVoice(It.IsAny<JournalEntryEntity>(), blobContainerClient, It.IsAny<CancellationToken>()), Times.AtLeastOnce);
        voiceover.Verify(x => x.DeleteLocalFile(It.IsAny<JournalEntryEntity>()), Times.AtLeastOnce);
        ingestiob.Verify(x => x.DeleteJournalEntry(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
        kinde.Verify(x => x.DeleteUser(userId, It.IsAny<CancellationToken>()), Times.Once);
        deleteUser.Verify(x => x.UpdateDeleteLog(It.IsAny<UserDeleteLogEntity>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
