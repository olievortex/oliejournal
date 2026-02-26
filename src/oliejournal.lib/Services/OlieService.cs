using Azure;
using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Google.Cloud.Speech.V1;
using Google.Cloud.TextToSpeech.V1;
using Newtonsoft.Json;
using oliejournal.lib.Services.Models;
using OpenAI.Responses;
using System.ClientModel;
using System.ClientModel.Primitives;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO.Compression;
using System.Text;
using System.Text.Json;

namespace oliejournal.lib.Services;

[ExcludeFromCodeCoverage]
public class OlieService : IOlieService
{
    #region Blob

    public async Task BlobDeleteFile(BlobContainerClient client, string fileName, CancellationToken ct)
    {
        try
        {
            var blobClient = client.GetBlobClient(fileName);
            await blobClient.DeleteIfExistsAsync(cancellationToken: ct);
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            // Do Nothing
        }
    }

    public async Task BlobDownloadFile(BlobContainerClient client, string fileName, string localFileName, CancellationToken ct)
    {
        var blobClient = client.GetBlobClient(fileName);
        await blobClient.DownloadToAsync(localFileName, ct);
    }

    public async Task BlobUploadFile(BlobContainerClient client, string fileName, string localFileName, CancellationToken ct)
    {
        var blobClient = client.GetBlobClient(fileName);
        var contentType = "application/octet-stream";
        var extension = Path.GetExtension(fileName);

        if (extension.Equals(".gif", StringComparison.OrdinalIgnoreCase)) contentType = "image/gif";
        if (extension.Equals(".mp4", StringComparison.OrdinalIgnoreCase)) contentType = "video/mp4";
        if (extension.Equals(".wav", StringComparison.OrdinalIgnoreCase)) contentType = "audio/wav";

        var headers = new BlobHttpHeaders
        {
            CacheControl = "public, max-age=604800",
            ContentType = contentType
        };

        await blobClient.UploadAsync(localFileName, headers, cancellationToken: ct);
    }

    #endregion

    #region Directory

    public List<string> DirectoryList(string path)
    {
        if (!Directory.Exists(path)) return [];
        var files = Directory.GetFiles(path).ToList();
        return files;
    }

    #endregion

    #region File

    public void FileCompressGzip(string sourceFile, string destinationFile)
    {
        using var originalFileStream = File.Open(sourceFile, FileMode.Open);
        using var compressedFileStream = File.Create(destinationFile);
        using var compressionStream = new GZipStream(compressedFileStream, CompressionLevel.Optimal);
        originalFileStream.CopyTo(compressionStream);
    }

    public void FileCreateDirectory(string path)
    {
        var directory = Path.GetDirectoryName(path);
        if (string.IsNullOrWhiteSpace(directory)) return;

        Directory.CreateDirectory(directory);
    }

    public void FileDelete(string path)
    {
        File.Delete(path);
    }

    public bool FileExists(string path)
    {
        return File.Exists(path);
    }

    public async Task FileWriteAllBytes(string path, byte[] data, CancellationToken ct)
    {
        await File.WriteAllBytesAsync(path, data, ct);
    }

    #endregion

    #region Ffmpeg

    public async Task FfmpegWavToMp3(string audioIn, string mp3Out, string ffmpegPath, CancellationToken ct)
    {
        var sbStdOut = new StringBuilder();
        var sbErrOut = new StringBuilder();

        using var ff = new Process();
        ff.StartInfo.UseShellExecute = false;
        ff.StartInfo.CreateNoWindow = true;
        ff.StartInfo.FileName = ffmpegPath;
        ff.StartInfo.RedirectStandardError = true;
        ff.ErrorDataReceived += (_, args) => sbErrOut.Append(args.Data);
        ff.StartInfo.RedirectStandardOutput = true;
        ff.OutputDataReceived += (_, args) => sbStdOut.Append(args.Data);
        ff.StartInfo.Arguments = $"-i {ToLinux(audioIn)} -c:a libmp3lame -qscale:a 2 {ToLinux(mp3Out)}";
        ff.Start();
        ff.BeginOutputReadLine();
        ff.BeginErrorReadLine();
        await ff.WaitForExitAsync(ct);

        if (ff.ExitCode != 0) throw new ApplicationException($"ffmpeg exit code {ff.ExitCode}: {sbStdOut}\n{sbErrOut}");
        ff.Close();

        return;

        static string ToLinux(string path)
        {
            return path.Replace("\\", "/");
        }
    }

    #endregion

    #region Google

    public async Task<byte[]> GoogleSpeak(string voiceName, string script, CancellationToken ct)
    {
        // Instantiates a client
        var client = TextToSpeechClient.Create();

        // Set the text input to be synthesized
        var input = new SynthesisInput
        {
            Text = script,
        };

        // Build the voice request, select the language code ("en-US") and the ssml voice gender
        // ("neutral")
        var voice = new VoiceSelectionParams
        {
            Name = voiceName,
            LanguageCode = "en-US",
        };

        // Select the type of audio file you want returned
        var audioConfig = new AudioConfig()
        {
            AudioEncoding = AudioEncoding.Linear16,
            SpeakingRate = 1.3,
        };

        // Perform the text-to-speech request on the text input with the selected voice parameters and
        // audio file type
        var response = await client.SynthesizeSpeechAsync(input, voice, audioConfig, ct);

        // Get the audio contents from the response
        return response.AudioContent.ToByteArray();
    }

    public async Task<OlieTranscribeResult> GoogleTranscribeWavNoEx(string localFile, OlieWavInfo info, CancellationToken ct)
    {
        const int serviceId = 1;

        try
        {
            var transcript = string.Empty;

            if (info.Channels != 1) throw new ApplicationException("WAV must be mono");
            if (info.BitsPerSample != 16) throw new ApplicationException("WAV must be 16 bit");

            var client = await SpeechClient.CreateAsync(ct);
            var audio = await RecognitionAudio.FromFileAsync(localFile);
            var config = new RecognitionConfig
            {
                Encoding = RecognitionConfig.Types.AudioEncoding.Linear16,
                SampleRateHertz = info.SampleRate,
                LanguageCode = LanguageCodes.English.UnitedStates,
                EnableAutomaticPunctuation = true,
                EnableSpokenPunctuation = false
            };
            var response = await client.RecognizeAsync(config, audio, ct);

            foreach (var item in response.Results)
            {
                if (item.Alternatives.Count > 0)
                {
                    var alternative = item.Alternatives[0];
                    transcript = alternative.Transcript;
                }
            }

            return new OlieTranscribeResult
            {
                Transcript = transcript,
                Cost = (int)response.TotalBilledTime.Seconds,
                ServiceId = serviceId,
            };
        }
        catch (Exception ex)
        {
            return new OlieTranscribeResult
            {
                Cost = (int)info.Duration.Seconds,
                Exception = ex,
                ServiceId = serviceId,
            };
        }
    }

    #endregion

    #region OpenAi

#pragma warning disable OPENAI001

    public async Task<string> OpenAiCreateConversation(string userId, string instructions, string apiKey, CancellationToken ct)
    {
        var client = new OpenAI.Conversations.ConversationClient(apiKey);
        var options = new RequestOptions() { CancellationToken = ct, };

        var content = BinaryContent.CreateJson(new
        {
            metadata = new Dictionary<string, string>
            {
                { "userId", userId}
            },
            items = new List<object>()
            {
                new
                {
                    content = instructions,
                    role = "developer",
                    type = "message",
                }
            }
        });

        var result = await client.CreateConversationAsync(content, options);

        using var resultJson = JsonDocument.Parse(result.GetRawResponse().Content.ToString());
        var conversationId = resultJson.RootElement
            .GetProperty("id"u8)
            .GetString()
            ?? throw new ApplicationException($"Unable to create new conversation for {userId}");

        return conversationId;
    }

    public async Task<OlieChatbotResult> OpenAiEngageChatbotNoEx(string userId, string message, string conversationId, string model, string apiKey, CancellationToken ct)
    {
        const int serviceId = 1;

        try
        {
            var client = new ResponsesClient(model, apiKey);

            var responseOptions = new CreateResponseOptions
            {
                ConversationOptions = new ResponseConversationOptions(conversationId),
            };
            responseOptions.InputItems.Add(ResponseItem.CreateUserMessageItem(message));
            responseOptions.Metadata.Add("userId", userId);

            ResponseResult response = await client.CreateResponseAsync(responseOptions, ct);

            var result = new OlieChatbotResult
            {
                ConversationId = conversationId,
                Message = response.GetOutputText() ?? string.Empty,
                ServiceId = serviceId,
                InputTokens = response.Usage.InputTokenCount,
                OutputTokens = response.Usage.OutputTokenCount,
                ResponseId = response.Id,
            };

            return result;
        }
        catch (Exception ex)
        {
            var result = new OlieChatbotResult
            {
                ConversationId = conversationId,
                Exception = ex,
                ServiceId = serviceId,
                InputTokens = (int)(message.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length * 1.3),
                OutputTokens = 200
            };

            return result;
        }
    }

    public async Task OpenAiDeleteConversation(string conversationId, string apiKey, CancellationToken ct)
    {
        try
        {
            var client = new OpenAI.Conversations.ConversationClient(apiKey);
            var options = new RequestOptions() { CancellationToken = ct, };
            await client.DeleteConversationAsync(conversationId, options);
        }
        catch (ClientResultException ex) when (ex.Status == 404)
        {
            // Do Nothing
        }
    }

#pragma warning restore OPENAI001

    #endregion

    #region ServiceBus

    public async Task ServiceBusSendJson(ServiceBusSender sender, object data, CancellationToken ct)
    {
        var json = JsonConvert.SerializeObject(data);
        var message = new ServiceBusMessage(json)
        {
            ContentType = "application/json"
        };

        await sender.SendMessageAsync(message, ct);
    }

    public async Task ServiceBusCompleteMessage<T>(ServiceBusReceiver receiver, OlieServiceBusReceivedMessage<T> message, CancellationToken ct)
    {
        await receiver.CompleteMessageAsync(message.ServiceBusReceivedMessage, ct);
    }

    public async Task<OlieServiceBusReceivedMessage<T>?> ServiceBusReceiveJson<T>(ServiceBusReceiver receiver, TimeSpan timeout, CancellationToken ct)
    {
        try
        {
            var message = await receiver.ReceiveMessageAsync(timeout, ct);
            if (message is null) return null;
            var json = message.Body.ToString();
            var body = JsonConvert.DeserializeObject<T>(json)
                ?? throw new InvalidCastException(json);

            return new OlieServiceBusReceivedMessage<T>
            {
                ServiceBusReceivedMessage = message,
                Body = body
            };
        }
        catch (TaskCanceledException)
        {
            return null;
        }
    }

    #endregion

    #region Stream

    public async Task<byte[]> StreamToByteArray(Stream stream, CancellationToken ct)
    {
        using var result = new MemoryStream();
        await stream.CopyToAsync(result, ct);

        return result.ToArray();
    }

    #endregion
}
