using oliejournal.data.Entities;
using oliejournal.lib.Services.Models;
using System.Diagnostics;

namespace oliejournal.lib.Units;

public interface IJournalEntryChatbotUnit
{
    Task<OlieChatbotResult> Chatbot(string message, string conversationId, CancellationToken ct);
    Task CreateJournalChatbot(int journalTranscriptId, OlieChatbotResult result, Stopwatch stopwatch, CancellationToken ct);
    Task DeleteConversations(string userId, CancellationToken ct);
    Task<ConversationEntity> GetConversation(string userId, CancellationToken ct);
    Task<JournalTranscriptEntity> GetJournalTranscriptOrThrow(int journalEntryId, CancellationToken ct);
    Task EnsureOpenAiLimit(int limit, CancellationToken ct);
    Task<bool> IsAlreadyChatbotted(int journalTranscriptId, CancellationToken ct);
}
