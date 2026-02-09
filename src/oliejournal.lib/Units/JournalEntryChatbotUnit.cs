using oliejournal.data;
using oliejournal.data.Entities;
using oliejournal.lib.Services;
using oliejournal.lib.Services.Models;
using System.Diagnostics;

namespace oliejournal.lib.Units;

public class JournalEntryChatbotUnit(IMyRepository repo, IOlieService os, IOlieConfig config) : IJournalEntryChatbotUnit
{
    public async Task<OlieChatbotResult> Chatbot(string message, string conversationId, CancellationToken ct)
    {
        return await os.OpenAiEngageChatbot(message, conversationId, ct);
    }

    public async Task CreateJournalChatbot(int journalTranscriptId, OlieChatbotResult result, Stopwatch stopwatch, CancellationToken ct)
    {
        var entity = new JournalChatbotEntity
        {
            ConversationFk = result.ConversationId,
            ServiceFk = result.ServiceId,
            JournalTranscriptFk = journalTranscriptId,

            ProcessingTime = (int)stopwatch.Elapsed.TotalSeconds,
            Message = result.Message?.Left(8096),
            InputTokens = 0,
            OutputTokens = 0,
            Exception = result.Exception?.ToString().Left(8096),
            Created = DateTime.UtcNow,
        };

        await repo.JournalChatbotCreate(entity, ct);
    }

    public async Task DeleteConversations(string userId, CancellationToken ct)
    {
        var conversations = await repo.ConversationGetActiveList(userId, ct);

        foreach (var conversation in conversations)
        {
            if ((DateTime.UtcNow - conversation.Timestamp).TotalDays > 3)
            {
                await os.OpenAiDeleteConversation(conversation.Id, ct);

                conversation.Deleted = DateTime.UtcNow;
                await repo.ConversationUpdate(conversation, ct);
            }
        }
    }

    public async Task<ConversationEntity> GetConversation(string userId, CancellationToken ct)
    {
        var conversation = (await repo.ConversationGetActiveList(userId, ct)).FirstOrDefault();
        if (conversation is not null) return conversation;

        var id = await os.OpenAiCreateConversation(userId, config.ChatbotInstructions, ct);

        var entity = new ConversationEntity
        {
            Id = id,

            UserId = userId,
            Created = DateTime.UtcNow,
            Timestamp = DateTime.UtcNow,
        };

        await repo.ConversationCreate(entity, ct);

        return entity;
    }

    public async Task<JournalTranscriptEntity> GetJournalTranscriptOrThrow(int journalEntryId, CancellationToken ct)
    {
        return await repo.JournalTranscriptGetByJournalEntryFk(journalEntryId, ct) ??
            throw new ApplicationException($"JournalTranscript for {journalEntryId} doesn't exist");
    }

    public async Task EnsureOpenAiLimit(int limit, CancellationToken ct)
    {
        const double inputRate = 0.2 / 1_000_000; // GPT-4.1 nano
        const double outputRate = 0.8 / 1_000_000; // GPT-4.1 nano

        var lookback = DateTime.UtcNow.AddMonths(-1);
        var billing = await repo.OpenApiGetChatbotSummary(lookback, ct);

        var cost = billing.InputTokens * inputRate + billing.OutputTokens * outputRate;

        if (cost > limit) throw new ApplicationException("OpenAi chat budget exceeded");
    }

    public async Task<bool> IsAlreadyChatbotted(int journalTranscriptId, CancellationToken ct)
    {
        return await repo.JournalChatbotGetByJournalTranscriptFk(journalTranscriptId, ct) is not null;
    }
}
