using Moq;
using oliejournal.data;
using oliejournal.data.Entities;
using oliejournal.lib.Processes.DeleteUserProcess;

namespace oliejournal.tests.ProcessTests.DeleteUserProcessTests;

public class DeleteUserUnitTests
{
    [Test]
    public async Task CreateDeleteLog_Should_Create_Entity_And_Call_Repository()
    {
        // Arrange
        var entity = new UserDeleteLogEntity();
        var repoMock = new Mock<IMyRepository>();
        repoMock.Setup(r => r.UserDeleteLogCreate(It.IsAny<UserDeleteLogEntity>(), It.IsAny<CancellationToken>()))
            .Callback<UserDeleteLogEntity, CancellationToken>((e, ct) =>
            {
                // Simulate repository setting the Id after creation
                e.Id = 123;
            });
        var deleteUnit = new DeleteUserUnit(repoMock.Object);
        var userId = "test-user";
        var requested = DateTime.UtcNow;

        // Act
        var result = await deleteUnit.CreateDeleteLog(userId, requested, CancellationToken.None);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Id, Is.EqualTo(123));
            Assert.That(result.UserId, Is.EqualTo(userId));
            Assert.That(result.Requested, Is.EqualTo(requested));
            Assert.That(result.DeleteViaApi, Is.True);
        }
        repoMock.Verify(r => r.UserDeleteLogCreate(It.Is<UserDeleteLogEntity>(e =>
            e.UserId == userId &&
            e.Requested == requested &&
            e.DeleteViaApi == true), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task UpdateDeleteLog_Should_Call_Repository_Update()
    {
        // Arrange
        var repoMock = new Mock<IMyRepository>();
        var deleteUnit = new DeleteUserUnit(repoMock.Object);
        var entity = new UserDeleteLogEntity
        {
            UserId = "test-user",
            Requested = DateTime.UtcNow.AddHours(-1),
            DeleteViaApi = true,
        };

        // Act
        await deleteUnit.UpdateDeleteLog(entity, CancellationToken.None);

        // Assert
        repoMock.Verify(r => r.UserDeleteLogUpdate(It.IsAny<UserDeleteLogEntity>(), CancellationToken.None), Times.Once);
    }
}
