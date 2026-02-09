using Azure.Messaging.ServiceBus;
using Moq;
using oliejournal.data;
using oliejournal.data.Entities;
using oliejournal.lib.Enums;
using oliejournal.lib.Models;
using oliejournal.lib.Services;
using oliejournal.lib.Services.Models;
using oliejournal.lib.Units;

namespace oliejournal.tests.UnitsTests;

[TestFixture]
public class JournalEntryIngestionUnitTests
{
    #region CreateJournalEntry

    [Test]
    public async Task CreateJournalEntry_SetsPropertiesAndCallsRepo()
    {
        // Arrange
        var owr = new Mock<IOlieWavReader>();
        var os = new Mock<IOlieService>();
        var repo = new Mock<IMyRepository>();
        repo.Setup(s => s.JournalEntryCreate(It.IsAny<JournalEntryEntity>(), CancellationToken.None))
            .Callback((JournalEntryEntity e, CancellationToken _) => { e.Id = 123; });

        var unit = new JournalEntryIngestionUnit(owr.Object, os.Object, repo.Object);

        var info = new OlieWavInfo
        {
            BitsPerSample = 16,
            Channels = 1,
            Duration = TimeSpan.FromSeconds(10),
            SampleRate = 16000
        };

        // Act
        var entity = await unit.CreateJournalEntry("user-1", info, "path.wav", 500, CancellationToken.None);

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
        }
    }

    #endregion

    #region CreateJournalMessage

    [Test]
    public async Task CreateJournalMessage_CallsServiceBusSendJsonWithCorrectModel()
    {
        // Arrange
        var model = new AudioProcessQueueItemModel();
        var owr = new Mock<IOlieWavReader>();
        var os = new Mock<IOlieService>();
        os.Setup(s => s.ServiceBusSendJson(null!, It.IsAny<object>(), CancellationToken.None))
            .Callback((ServiceBusSender _, object m, CancellationToken __) => { model = (AudioProcessQueueItemModel)m; });
        var repo = new Mock<IMyRepository>();

        var unit = new JournalEntryIngestionUnit(owr.Object, os.Object, repo.Object);

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
        var owr = new Mock<IOlieWavReader>();
        var os = new Mock<IOlieService>();
        var repo = new Mock<IMyRepository>();

        var unit = new JournalEntryIngestionUnit(owr.Object, os.Object, repo.Object);

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
        var owr = new Mock<IOlieWavReader>();
        var os = new Mock<IOlieService>();
        var repo = new Mock<IMyRepository>();

        var unit = new JournalEntryIngestionUnit(owr.Object, os.Object, repo.Object);

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
        var owr = new Mock<IOlieWavReader>();
        owr.Setup(s => s.GetOlieWavInfo(It.IsAny<byte[]>()))
            .Returns(new OlieWavInfo { Channels = 2, SampleRate = 16000, BitsPerSample = 16, Duration = TimeSpan.FromSeconds(10) });
        var os = new Mock<IOlieService>();
        var repo = new Mock<IMyRepository>();
        var unit = new JournalEntryIngestionUnit(owr.Object, os.Object, repo.Object);

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
        var owr = new Mock<IOlieWavReader>();
        owr.Setup(s => s.GetOlieWavInfo(It.IsAny<byte[]>()))
            .Returns(new OlieWavInfo { Channels = 1, SampleRate = 7000, BitsPerSample = 16, Duration = TimeSpan.FromSeconds(10) });
        var os = new Mock<IOlieService>();
        var repo = new Mock<IMyRepository>();
        var unit = new JournalEntryIngestionUnit(owr.Object, os.Object, repo.Object);

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
        var owr = new Mock<IOlieWavReader>();
        owr.Setup(s => s.GetOlieWavInfo(It.IsAny<byte[]>()))
            .Returns(new OlieWavInfo { Channels = 1, SampleRate = 16000, BitsPerSample = 8, Duration = TimeSpan.FromSeconds(10) });
        var os = new Mock<IOlieService>();
        var repo = new Mock<IMyRepository>();
        var unit = new JournalEntryIngestionUnit(owr.Object, os.Object, repo.Object);

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
        var owr = new Mock<IOlieWavReader>();
        owr.Setup(s => s.GetOlieWavInfo(It.IsAny<byte[]>()))
            .Returns(new OlieWavInfo { Channels = 1, SampleRate = 16000, BitsPerSample = 16, Duration = TimeSpan.FromSeconds(56) });
        var os = new Mock<IOlieService>();
        var repo = new Mock<IMyRepository>();
        var unit = new JournalEntryIngestionUnit(owr.Object, os.Object, repo.Object);

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
        var owr = new Mock<IOlieWavReader>();
        owr.Setup(s => s.GetOlieWavInfo(It.IsAny<byte[]>()))
            .Returns(info);
        var os = new Mock<IOlieService>();
        var repo = new Mock<IMyRepository>();
        var unit = new JournalEntryIngestionUnit(owr.Object, os.Object, repo.Object);

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
        var owr = new Mock<IOlieWavReader>();
        var os = new Mock<IOlieService>();
        os.Setup(s => s.StreamToByteArray(It.IsAny<Stream>(), CancellationToken.None))
            .ReturnsAsync(result);
        var repo = new Mock<IMyRepository>();
        var unit = new JournalEntryIngestionUnit(owr.Object, os.Object, repo.Object);

        using var ms = new MemoryStream([9, 8, 7]);

        // Act
        var bytes = await unit.GetBytesFromStream(ms, CancellationToken.None);

        // Assert
        Assert.That(result, Is.EqualTo(bytes));
    }

    #endregion

    #region WriteAudioFileToBlob

    [Test]
    public async Task WriteAudioFileToBlob_ComputesBlobPathAndCallsUpload()
    {
        // Arrange
        var owr = new Mock<IOlieWavReader>();
        var os = new Mock<IOlieService>();
        var repo = new Mock<IMyRepository>();
        var unit = new JournalEntryIngestionUnit(owr.Object, os.Object, repo.Object);

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
        var owr = new Mock<IOlieWavReader>();
        var os = new Mock<IOlieService>();
        var repo = new Mock<IMyRepository>();
        var unit = new JournalEntryIngestionUnit(owr.Object, os.Object, repo.Object);

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