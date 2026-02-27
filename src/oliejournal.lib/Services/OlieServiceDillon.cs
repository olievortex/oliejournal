using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;
using oliejournal.lib.Services.Models;
using System.Diagnostics.CodeAnalysis;

namespace oliejournal.lib.Services;

[ExcludeFromCodeCoverage]
public class OlieServiceDillon : OlieService
{
    private readonly Random _random = new();

    #region Blob

    public override async Task BlobDeleteFile(BlobContainerClient client, string fileName, CancellationToken ct)
    {
        await Task.Delay(_random.Next(50, 200), ct);
    }

    public override Task BlobDownloadFile(BlobContainerClient client, string fileName, string localFileName, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public override async Task BlobUploadFile(BlobContainerClient client, string fileName, string localFileName, CancellationToken ct)
    {
        await Task.Delay(_random.Next(250, 1000), ct);
    }

    #endregion

    #region Directory

    public override List<string> DirectoryList(string path)
    {
        throw new NotImplementedException();
    }

    #endregion

    #region Ffmpeg

    public override Task FfmpegWavToMp3(string audioIn, string mp3Out, string ffmpegPath, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    #endregion

    #region File

    public override void FileCompressGzip(string sourceFile, string destinationFile)
    {
        throw new NotImplementedException();
    }

    public override void FileCreateDirectory(string path)
    {
        throw new NotImplementedException();
    }

    public override void FileDelete(string path)
    {
        Thread.Sleep(_random.Next(50, 200));
    }

    public override bool FileExists(string path)
    {
        throw new NotImplementedException();
    }

    public override async Task FileWriteAllBytes(string path, byte[] data, CancellationToken ct)
    {
        await Task.Delay(_random.Next(50, 200), ct);
    }

    #endregion

    #region Google

    public override Task<byte[]> GoogleSpeak(string voiceName, string script, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public override Task<OlieTranscribeResult> GoogleTranscribeWavNoEx(string localFile, OlieWavInfo info, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    #endregion

    #region OpenAi

    public override Task<string> OpenAiCreateConversation(string userId, string instructions, string apiKey, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public override Task OpenAiDeleteConversation(string conversationId, string apiKey, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public override Task<OlieChatbotResult> OpenAiEngageChatbotNoEx(string userId, string message, string conversationId, string model, string apiKey, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    #endregion

    #region ServiceBus

    public override Task ServiceBusCompleteMessage<T>(ServiceBusReceiver receiver, OlieServiceBusReceivedMessage<T> message, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public override Task<OlieServiceBusReceivedMessage<T>?> ServiceBusReceiveJson<T>(ServiceBusReceiver receiver, TimeSpan timeout, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public override async Task ServiceBusSendJson(ServiceBusSender sender, object data, CancellationToken ct)
    {
        await Task.Delay(_random.Next(50, 200), ct);
    }

    #endregion
}
