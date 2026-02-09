using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;
using oliejournal.lib.Enums;
using oliejournal.lib.Units;
using System.Diagnostics;

namespace oliejournal.lib;

public class JournalProcess(
    IJournalEntryIngestionUnit ingestion,
    IJournalEntryTranscribeUnit transcribe,
    IJournalEntryChatbotUnit chatbot) : IJournalProcess
{
    const int GoogleApiLimit = 5;
    const int OpenAiLimit = 5;

    public async Task ChatbotAudioEntry(int journalEntryId, ServiceBusSender sender, CancellationToken ct)
    {
        var entry = await transcribe.GetJournalEntryOrThrow(journalEntryId, ct);
        var transcript = await chatbot.GetJournalTranscriptOrThrow(journalEntryId, ct);
        if (await chatbot.IsAlreadyChatbotted(journalEntryId, ct)) goto SendMessage;
        if (transcript.Transcript is null) goto SendMessage;

        await chatbot.EnsureOpenAiLimit(OpenAiLimit, ct);
        await chatbot.DeleteConversations(entry.UserId, ct);
        var conversation = await chatbot.GetConversation(entry.UserId, ct);

        var stopwatch = Stopwatch.StartNew();
        var message = await chatbot.Chatbot(transcript.Transcript, conversation.Id, ct);
        await chatbot.CreateJournalChatbot(journalEntryId, message, stopwatch, ct);

        if (message.Exception is not null) throw message.Exception;

        SendMessage:
        await ingestion.CreateJournalMessage(journalEntryId, AudioProcessStepEnum.VoiceOver, sender, ct);
    }

    public async Task<int> IngestAudioEntry(string userId, Stream audio, ServiceBusSender sender, BlobContainerClient client, CancellationToken ct)
    {
        var file = await ingestion.GetBytesFromStream(audio, ct);

        var wavInfo = ingestion.EnsureAudioValidates(file);
        var localPath = await ingestion.WriteAudioFileToTemp(file, ct);
        var blobPath = await ingestion.WriteAudioFileToBlob(localPath, client, ct);
        var entry = await ingestion.CreateJournalEntry(userId, wavInfo, blobPath, file.Length, ct);
        await ingestion.CreateJournalMessage(entry.Id, AudioProcessStepEnum.Transcript, sender, ct);

        return entry.Id;
    }

    public async Task TranscribeAudioEntry(int journalEntryId, BlobContainerClient client, ServiceBusSender sender, CancellationToken ct)
    {
        var entity = await transcribe.GetJournalEntryOrThrow(journalEntryId, ct);
        if (await transcribe.IsAlreadyTranscribed(journalEntryId, ct)) goto SendMessage;

        await transcribe.EnsureGoogleLimit(GoogleApiLimit, ct);
        var localFile = await transcribe.GetAudioFile(entity.AudioPath, client, ct);

        var stopwatch = Stopwatch.StartNew();
        var transcript = await transcribe.Transcribe(localFile, ct);
        await transcribe.CreateJournalTranscript(journalEntryId, transcript, stopwatch, ct);

        if (transcript.Exception is not null) throw transcript.Exception;

        transcribe.Cleanup(localFile);

    SendMessage:
        await ingestion.CreateJournalMessage(journalEntryId, AudioProcessStepEnum.Chatbot, sender, ct);
    }
}
