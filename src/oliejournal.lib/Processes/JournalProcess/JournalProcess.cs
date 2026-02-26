using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;
using oliejournal.lib.Enums;
using oliejournal.lib.Models;
using System.Diagnostics;

namespace oliejournal.lib.Processes.JournalProcess;

public class JournalProcess(
    IJournalEntryIngestionUnit ingestion,
    IJournalEntryTranscribeUnit transcribe,
    IJournalEntryChatbotUnit chatbot,
    IJournalEntryVoiceoverUnit voiceover) : IJournalProcess
{
    const int GoogleApiLimit = 5;
    const int OpenAiLimit = 5;

    public async Task Voiceover(int journalEntryId, CancellationToken ct)
    {
        var entry = await ingestion.GetJournalEntryOrThrow(journalEntryId, ct);
        if (entry.VoiceoverPath != null) return;
        if (entry.Response is null) throw new ApplicationException($"Chatbot response null for {journalEntryId}");

        var file = await voiceover.VoiceOver(entry.Response, ct);
        var blobPath = await voiceover.SaveLocalFile(file, ct);
        var wavInfo = await voiceover.GetWavInfo(file);

        await voiceover.UpdateEntry(blobPath, file.Length, wavInfo, entry, ct);
    }

    public async Task Chatbot(int journalEntryId, ServiceBusSender sender, CancellationToken ct)
    {
        var entry = await ingestion.GetJournalEntryOrThrow(journalEntryId, ct);
        if (entry.Response is not null) goto SendMessage;
        if (string.IsNullOrWhiteSpace(entry.Transcript)) throw new ApplicationException($"Transcript is empty for {journalEntryId}");

        await chatbot.EnsureOpenAiLimit(OpenAiLimit, ct);
        await chatbot.DeleteOldConversations(entry.UserId, ct);
        var conversation = await chatbot.GetConversation(entry.UserId, ct);

        var stopwatch = Stopwatch.StartNew();
        var message = await chatbot.Chatbot(entry.UserId, entry.Transcript, conversation.Id, ct);
        await chatbot.CreateChatbotLog(entry.Id, message, stopwatch, ct);

        if (message.Exception is not null) throw message.Exception;
        if (message.Message is null) throw new ApplicationException($"Chatbot response null for {journalEntryId}");

        await chatbot.UpdateEntry(message.Message, entry, ct);

    SendMessage:
        await ingestion.CreateJournalMessage(journalEntryId, AudioProcessStepEnum.VoiceOver, sender, ct);
    }

    public async Task<List<JournalEntryListModel>> GetEntryList(string userId, CancellationToken ct)
    {
        return [.. (await ingestion.GetJournalEntryList(userId, ct)).Select(JournalEntryListModel.FromEntity)];
    }

    public async Task<JournalEntryListModel?> GetEntry(int journalEntryId, string userId, CancellationToken ct)
    {
        var result = await ingestion.GetJournalEntry(journalEntryId, userId, ct);

        return result is null ? null : JournalEntryListModel.FromEntity(result);
    }

    public async Task<int> Ingest(string userId, Stream audio, float? latitude, float? longitude, ServiceBusSender sender, BlobContainerClient client, CancellationToken ct)
    {
        var file = await ingestion.GetBytesFromStream(audio, ct);
        var hash = ingestion.CreateHash(file);

        var entry = await ingestion.GetDuplicateEntry(userId, hash, ct);
        if (entry != null) goto SendMessage;

        var wavInfo = ingestion.EnsureAudioValidates(file);
        var localPath = await ingestion.WriteAudioFileToTemp(file, ct);
        var blobPath = await ingestion.WriteAudioFileToBlob(localPath, client, ct);
        entry = await ingestion.CreateJournalEntry(userId, blobPath, file.Length, hash, latitude, longitude, wavInfo, ct);

    SendMessage:
        await ingestion.CreateJournalMessage(entry.Id, AudioProcessStepEnum.Transcript, sender, ct);

        return entry.Id;
    }

    public async Task Transcribe(int journalEntryId, BlobContainerClient client, ServiceBusSender sender, CancellationToken ct)
    {
        var entity = await ingestion.GetJournalEntryOrThrow(journalEntryId, ct);
        if (!string.IsNullOrWhiteSpace(entity.Transcript)) goto SendMessage;

        await transcribe.EnsureGoogleLimit(GoogleApiLimit, ct);
        var localFile = await transcribe.GetAudioFile(entity.AudioPath, client, ct);

        var stopwatch = Stopwatch.StartNew();
        var transcript = await transcribe.Transcribe(localFile, ct);
        await transcribe.CreateTranscriptLog(journalEntryId, transcript, stopwatch, ct);

        if (transcript.Exception is not null) throw transcript.Exception;
        if (transcript.Transcript is null) throw new ApplicationException($"Transcript null for {journalEntryId}");

        await transcribe.UpdateEntry(transcript.Transcript, entity, ct);

        transcribe.Cleanup(localFile);

    SendMessage:
        await ingestion.CreateJournalMessage(journalEntryId, AudioProcessStepEnum.Chatbot, sender, ct);
    }

    public async Task<bool> DeleteEntry(int journalEntryId, string userId, BlobContainerClient client, CancellationToken ct)
    {
        // Verify ownership
        var entry = await ingestion.GetJournalEntry(journalEntryId, userId, ct);
        if (entry is null) return false;

        // Delete any ongoing OpenAI conversations
        await chatbot.DeleteAllConversations(userId, ct);

        // Delete original voice
        await ingestion.DeleteVoice(entry, client, ct);

        // Delete chatbot voiceover
        voiceover.DeleteLocalFile(entry);

        // Finally delete JournalEntry
        await ingestion.DeleteJournalEntry(journalEntryId, ct);

        return true;
    }
}
