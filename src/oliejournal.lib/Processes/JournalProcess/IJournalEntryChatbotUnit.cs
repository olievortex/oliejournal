using oliejournal.data.Entities;
using oliejournal.lib.Services.Models;
using System.Diagnostics;

namespace oliejournal.lib.Processes.JournalProcess;

public interface IJournalEntryChatbotUnit
{
    Task<OlieChatbotResult> Chatbot(string userId, string message, string conversationId, CancellationToken ct);
    Task CreateChatbotLog(int journalTranscriptId, OlieChatbotResult result, Stopwatch stopwatch, CancellationToken ct);
    Task DeleteConversations(string userId, CancellationToken ct);
    Task DeleteJournalTranscript(int id, CancellationToken ct);
    Task<ChatbotConversationEntity> GetConversation(string userId, CancellationToken ct);
    Task<JournalTranscriptEntity> GetJournalTranscriptOrThrow(int journalEntryId, CancellationToken ct);
    Task<List<JournalTranscriptEntity>> GetJournalTranscripts(int journalEntryId, CancellationToken ct);
    Task EnsureOpenAiLimit(int limit, CancellationToken ct);
    Task UpdateEntry(string message, JournalEntryEntity entry, CancellationToken ct);
}
