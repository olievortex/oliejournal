using oliejournal.data;
using oliejournal.data.Entities;
using oliejournal.lib.Services;
using oliejournal.lib.Services.Models;
using System.Diagnostics;

namespace oliejournal.lib.Processes.JournalProcess;

public class JournalEntryChatbotUnit(IMyRepository repo, IOlieService os, IOlieConfig config) : IJournalEntryChatbotUnit
{
    public async Task<OlieChatbotResult> Chatbot(string userId, string message, string conversationId, CancellationToken ct)
    {
        return await os.OpenAiEngageChatbotNoEx(userId, message, conversationId, config.OpenAiModel, config.OpenAiApiKey, ct);
    }

    public async Task CreateChatbotLog(int journalTranscriptId, OlieChatbotResult result, Stopwatch stopwatch, CancellationToken ct)
    {
        var entity = new ChatbotLogEntity
        {
            ConversationId = result.ConversationId,
            ServiceId = result.ServiceId,
            JournalTranscriptFk = journalTranscriptId,

            ProcessingTime = (int)stopwatch.Elapsed.TotalSeconds,
            InputTokens = result.InputTokens,
            OutputTokens = result.OutputTokens,
            Exception = result.Exception?.ToString(),
            Created = DateTime.UtcNow,
            ResponseId = result.ResponseId,
        };

        await repo.ChatbotLogCreate(entity, ct);
    }

    public async Task DeleteConversations(string userId, CancellationToken ct)
    {
        var conversations = await repo.ChatbotConversationGetActiveList(userId, ct);

        foreach (var conversation in conversations)
        {
            if ((DateTime.UtcNow - conversation.Timestamp).TotalDays > 3)
            {
                await os.OpenAiDeleteConversation(conversation.Id, config.OpenAiApiKey, ct);
                await repo.ChatbotConversationDelete(conversation.Id, ct);
            }
        }
    }

    public async Task<ChatbotConversationEntity> GetConversation(string userId, CancellationToken ct)
    {
        var conversation = (await repo.ChatbotConversationGetActiveList(userId, ct)).FirstOrDefault();
        if (conversation is not null) return conversation;

        var id = await os.OpenAiCreateConversation(userId, config.ChatbotInstructions, config.OpenAiApiKey, ct);

        var entity = new ChatbotConversationEntity
        {
            Id = id,

            UserId = userId,
            Created = DateTime.UtcNow,
            Timestamp = DateTime.UtcNow,
        };

        await repo.ChatbotConversationCreate(entity, ct);

        return entity;
    }

    public async Task EnsureOpenAiLimit(int limit, CancellationToken ct)
    {
        const double inputRate = 0.2 / 1_000_000; // GPT-4.1 nano
        const double outputRate = 0.8 / 1_000_000; // GPT-4.1 nano

        var lookback = DateTime.UtcNow.AddMonths(-1);
        var billing = await repo.ChatbotLogSummary(lookback, ct);

        var cost = billing.InputTokens * inputRate + billing.OutputTokens * outputRate;

        if (cost > limit) throw new ApplicationException("OpenAi chat budget exceeded");
    }

    public async Task UpdateEntry(string message, JournalEntryEntity entry, CancellationToken ct)
    {
        entry.Response = message;
        entry.ResponseCreated = DateTime.UtcNow;

        await repo.JournalEntryUpdate(entry, ct);
    }
}
