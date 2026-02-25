using oliejournal.data.Entities;
using oliejournal.lib.Services.Models;
using System.Diagnostics;

namespace oliejournal.lib.Processes.JournalProcess;

public interface IJournalEntryChatbotUnit
{
    Task<OlieChatbotResult> Chatbot(string userId, string message, string conversationId, CancellationToken ct);
    Task CreateJournalChatbot(int journalTranscriptId, OlieChatbotResult result, Stopwatch stopwatch, CancellationToken ct);
    Task DeleteOpenAIConversations(string userId, CancellationToken ct);
    Task DeleteJournalChatbot(int id, CancellationToken ct);
    Task DeleteJournalTranscript(int id, CancellationToken ct);
    Task<ConversationEntity> GetConversation(string userId, CancellationToken ct);
    Task<List<JournalChatbotEntity>> GetJournalChatbots(int journalTranscriptId, CancellationToken ct);
    Task<JournalTranscriptEntity> GetJournalTranscriptOrThrow(int journalEntryId, CancellationToken ct);
    Task<List<JournalTranscriptEntity>> GetJournalTranscripts(int journalEntryId, CancellationToken ct);
    Task EnsureOpenAiLimit(int limit, CancellationToken ct);
    Task<bool> IsAlreadyChatbotted(int journalTranscriptId, CancellationToken ct);
}
