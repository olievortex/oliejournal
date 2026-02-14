using Azure.Messaging.ServiceBus;
using Moq;
using oliejournal.data;
using oliejournal.data.Entities;
using oliejournal.lib.Enums;
using oliejournal.lib.Models;
using oliejournal.lib.Processes.JournalProcess;
using oliejournal.lib.Services;
using oliejournal.lib.Services.Models;

namespace oliejournal.tests.ProcessTests.JournalProcessTests;

[TestFixture]
public class JournalEntryIngestionUnitTests
{
    private static (JournalEntryIngestionUnit, Mock<IOlieWavReader>, Mock<IOlieService>, Mock<IMyRepository>) CreateUnit()
    {
        var owr = new Mock<IOlieWavReader>();
        var os = new Mock<IOlieService>();
        var repo = new Mock<IMyRepository>();

        var unit = new JournalEntryIngestionUnit(owr.Object, os.Object, repo.Object);

        return (unit, owr, os, repo);
    }

    #region CreateHash

    [Test]
    public void CreateHash_ReturnsHash_ForBytes()
    {
        // Arrange
        var (unit, _, _, _) = CreateUnit();
        var file = new byte[] { 10, 20, 30, 40 };

        // Act
        var result = unit.CreateHash(file);

        // Assert
        Assert.That(result, Is.EqualTo("78735557df91f7b1232f0b0522bf5002"));
    }

    #endregion

    #region CreateJournalEntry

    [Test]
    public async Task CreateJournalEntry_SetsPropertiesAndCallsRepo()
    {
        // Arrange
        var (unit, _, _, repo) = CreateUnit();
        repo.Setup(s => s.JournalEntryCreate(It.IsAny<JournalEntryEntity>(), CancellationToken.None))
            .Callback<JournalEntryEntity, CancellationToken>((e, _) => { e.Id = 123; });

        var info = new OlieWavInfo
        {
            BitsPerSample = 16,
            Channels = 1,
            Duration = TimeSpan.FromSeconds(10),
            SampleRate = 16000
        };

        // Act
        var entity = await unit.CreateJournalEntry("user-1", "path.wav", 500, "A5DF", info, CancellationToken.None);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(entity.Id, Is.EqualTo(123));
            Assert.That(entity.UserId, Is.EqualTo("user-1"));
            Assert.That(entity.AudioBitsPerSample, Is.EqualTo(16));
            Assert.That(entity.AudioChannels, Is.EqualTo(1));
            Assert.That(entity.AudioLength, Is.EqualTo(500));
            Assert.That(entity.AudioSampleRate, Is.EqualTo(16000));
            Assert.That(entity.AudioPath, Is.EqualTo("path.wav"));
            Assert.That(entity.Created, Is.Not.EqualTo(DateTime.MinValue));
            Assert.That(entity.AudioDuration, Is.EqualTo((int)info.Duration.TotalSeconds));
            Assert.That(entity.AudioHash, Is.EqualTo("A5DF"));
        }
    }

    #endregion

    #region CreateJournalMessage

    [Test]
    public async Task CreateJournalMessage_CallsServiceBusSendJsonWithCorrectModel()
    {
        // Arrange
        var model = new AudioProcessQueueItemModel();
        var (unit, _, os, _) = CreateUnit();
        os.Setup(s => s.ServiceBusSendJson(null!, It.IsAny<object>(), CancellationToken.None))
            .Callback<ServiceBusSender, object, CancellationToken>((_, m, _) => { model = (AudioProcessQueueItemModel)m; });

        // Act
        await unit.CreateJournalMessage(77, AudioProcessStepEnum.Chatbot, null!, CancellationToken.None);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(model.Id, Is.EqualTo(77));
            Assert.That(model.Step, Is.EqualTo(AudioProcessStepEnum.Chatbot));
        }
    }

    #endregion

    #region EnsureAudioValidates

    [Test]
    public void EnsureAudioValidates_ThrowsWhenEmpty()
    {
        // Arrange
        var (unit, _, _, _) = CreateUnit();
        var empty = Array.Empty<byte>();

        // Act
        var ex = Assert.Throws<ApplicationException>(() => unit.EnsureAudioValidates(empty));

        // Assert
        Assert.That(ex!.Message, Is.EqualTo("WAV file empty"));
    }

    [Test]
    public void EnsureAudioValidates_ThrowsWhenTooLarge()
    {
        // Arrange
        var (unit, _, _, _) = CreateUnit();
        var large = new byte[9 * 1024 * 1024 + 1];

        // Act
        var ex = Assert.Throws<ApplicationException>(() => unit.EnsureAudioValidates(large));

        // Assert
        Assert.That(ex!.Message, Does.Contain("> 9MB"));
    }

    [Test]
    public void EnsureAudioValidates_ThrowsOnChannelsGreaterThanOne()
    {
        // Arrange
        var (unit, owr, _, _) = CreateUnit();
        owr.Setup(s => s.GetOlieWavInfo(It.IsAny<byte[]>()))
            .Returns(new OlieWavInfo { Channels = 2, SampleRate = 16000, BitsPerSample = 16, Duration = TimeSpan.FromSeconds(10) });
        var file = new byte[100];

        // Act
        var ex = Assert.Throws<ApplicationException>(() => unit.EnsureAudioValidates(file));

        // Assert
        Assert.That(ex!.Message, Does.Contain("channels"));
    }

    [Test]
    public void EnsureAudioValidates_ThrowsOnBadSampleRate()
    {
        // Arrange
        var (unit, owr, _, _) = CreateUnit();
        owr.Setup(s => s.GetOlieWavInfo(It.IsAny<byte[]>()))
            .Returns(new OlieWavInfo { Channels = 1, SampleRate = 7000, BitsPerSample = 16, Duration = TimeSpan.FromSeconds(10) });
        var file = new byte[100];

        // Act
        var ex = Assert.Throws<ApplicationException>(() => unit.EnsureAudioValidates(file));

        // Assert
        Assert.That(ex!.Message, Does.Contain("sample rate"));
    }

    [Test]
    public void EnsureAudioValidates_ThrowsOnBitsPerSampleNot16()
    {
        // Arrange
        var (unit, owr, _, _) = CreateUnit();
        owr.Setup(s => s.GetOlieWavInfo(It.IsAny<byte[]>()))
            .Returns(new OlieWavInfo { Channels = 1, SampleRate = 16000, BitsPerSample = 8, Duration = TimeSpan.FromSeconds(10) });
        var file = new byte[100];

        // Act
        var ex = Assert.Throws<ApplicationException>(() => unit.EnsureAudioValidates(file));

        // Assert
        Assert.That(ex!.Message, Does.Contain("bits per sample"));
    }

    [Test]
    public void EnsureAudioValidates_ThrowsOnDurationTooLong()
    {
        // Arrange
        var (unit, owr, _, _) = CreateUnit();
        owr.Setup(s => s.GetOlieWavInfo(It.IsAny<byte[]>()))
            .Returns(new OlieWavInfo { Channels = 1, SampleRate = 16000, BitsPerSample = 16, Duration = TimeSpan.FromSeconds(56) });
        var file = new byte[100];

        // Act
        var ex = Assert.Throws<ApplicationException>(() => unit.EnsureAudioValidates(file));

        // Assert
        Assert.That(ex!.Message, Does.Contain("duration"));
    }

    [Test]
    public void EnsureAudioValidates_ReturnsInfoWhenValid()
    {
        // Arrange
        var info = new OlieWavInfo { Channels = 1, SampleRate = 16000, BitsPerSample = 16, Duration = TimeSpan.FromSeconds(10) };
        var (unit, owr, _, _) = CreateUnit();
        owr.Setup(s => s.GetOlieWavInfo(It.IsAny<byte[]>()))
            .Returns(info);

        var file = new byte[100];

        // Act
        var result = unit.EnsureAudioValidates(file);

        // Assert
        Assert.That(info, Is.EqualTo(result));
    }

    #endregion

    #region GetBytesFromStream

    [Test]
    public async Task GetBytesFromStream_DelegatesToService()
    {
        // Arrange
        var result = new byte[] { 1, 2, 3 };
        var (unit, _, os, _) = CreateUnit();
        os.Setup(s => s.StreamToByteArray(It.IsAny<Stream>(), CancellationToken.None))
            .ReturnsAsync(result);

        using var ms = new MemoryStream([9, 8, 7]);

        // Act
        var bytes = await unit.GetBytesFromStream(ms, CancellationToken.None);

        // Assert
        Assert.That(result, Is.EqualTo(bytes));
    }

    #endregion

    #region GetDuplicateEntry

    [Test]
    public async Task GetDuplicateEntry_ReturnsDuplicate_WhenFound()
    {
        // Arrange
        const string userId = "abc";
        const string hash = "bcd";
        var duplicate = new JournalEntryEntity();
        var (unit, _, _, repo) = CreateUnit();
        repo.Setup(s => s.JournalEntryGetByHash(userId, hash, CancellationToken.None))
            .ReturnsAsync(duplicate);

        // Act
        var result = await unit.GetDuplicateEntry(userId, hash, CancellationToken.None);

        // Assert
        Assert.That(result, Is.EqualTo(duplicate));
    }

    #endregion

    #region WriteAudioFileToBlob

    [Test]
    public async Task WriteAudioFileToBlob_ComputesBlobPathAndCallsUpload()
    {
        // Arrange
        var (unit, _, _, _) = CreateUnit();

        var localPath = Path.Combine("some", "dir", "audio.wav");

        // Act
        var result = await unit.WriteAudioFileToBlob(localPath, client: null!, CancellationToken.None);

        // Assert
        Assert.That(result, Does.Contain("bronze/audio_entry/"));
        Assert.That(result, Does.EndWith(Path.GetFileName(localPath)));
    }

    #endregion

    #region WriteAudioFileToTemp

    [Test]
    public async Task WriteAudioFileToTemp_WritesFileAndReturnsPath()
    {
        // Arrange
        var (unit, _, _, _) = CreateUnit();

        var data = new byte[] { 5, 6, 7 };

        // Act
        var path = await unit.WriteAudioFileToTemp(data, CancellationToken.None);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(path, Does.EndWith(".wav"));
            Assert.That(Path.GetTempPath(), Is.Not.Null.And.Not.Empty);
            Assert.That(path, Does.Contain(Path.GetTempPath().TrimEnd(Path.DirectorySeparatorChar)));
        }
    }

    #endregion
}