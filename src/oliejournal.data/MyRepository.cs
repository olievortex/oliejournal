using Microsoft.EntityFrameworkCore;
using oliejournal.data.Entities;
using oliejournal.data.Models;
using System.Diagnostics.CodeAnalysis;

namespace oliejournal.data;

[ExcludeFromCodeCoverage]
public class MyRepository(MyContext context) : IMyRepository
{
    #region Conversation

    public async Task ConversationCreate(ConversationEntity entity, CancellationToken ct)
    {
        await context.Conversations.AddAsync(entity, ct);
        await context.SaveChangesAsync(ct);
    }

    public async Task<List<ConversationEntity>> ConversationGetActiveList(string userId, CancellationToken ct)
    {
        return await context.Conversations
            .Where(w => w.UserId == userId && w.Deleted == null)
            .OrderBy(o => o.Timestamp)
            .ToListAsync(ct);
    }

    public async Task ConversationUpdate(ConversationEntity entity, CancellationToken ct)
    {
        context.Conversations.Update(entity);
        await context.SaveChangesAsync(ct);
    }

    #endregion

    #region JournalChatbot

    public async Task JournalChatbotCreate(JournalChatbotEntity entity, CancellationToken ct)
    {
        await context.JournalChatbots.AddAsync(entity, ct);
        await context.SaveChangesAsync(ct);
    }

    public async Task<JournalChatbotEntity?> JournalChatbotGetByJournalEntryId(int journalEntryId, CancellationToken ct)
    {
        return await context
            .JournalTranscripts
            .Join(context.JournalChatbots, l => l.Id, r => r.JournalTranscriptFk, (t, c) => new { t, c })
            .Where(w => w.t.JournalEntryFk == journalEntryId && w.c.Message != null)
            .Select(s => s.c)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<JournalChatbotEntity?> JournalChatbotGetByJournalTranscriptFk(int journalTranscriptFk, CancellationToken ct)
    {
        return await context.JournalChatbots
            .Where(w => w.JournalTranscriptFk == journalTranscriptFk && w.Message != null)
            .SingleOrDefaultAsync(ct);
    }

    #endregion

    #region JournalEntry

    public async Task JournalEntryCreate(JournalEntryEntity entity, CancellationToken ct)
    {
        await context.JournalEntries.AddAsync(entity, ct);
        await context.SaveChangesAsync(ct);
    }

    public async Task<JournalEntryEntity?> JournalEntryGet(int id, CancellationToken ct)
    {
        return await context.JournalEntries.SingleOrDefaultAsync(s => s.Id == id, ct);
    }

    public async Task<JournalEntryEntity?> JournalEntryGetByHash(string userId, string hash, CancellationToken ct)
    {
        return await context.JournalEntries
            .FirstOrDefaultAsync(s => s.UserId == userId && s.AudioHash == hash, ct);
    }

    public async Task JournalEntryUpdate(JournalEntryEntity entity, CancellationToken ct)
    {
        context.JournalEntries.Update(entity);
        await context.SaveChangesAsync(ct);
    }

    #endregion

    #region JournalEntryList

    public async Task<JournalEntryListEntity?> JournalEntryListGetByUserId(int journalEntryId, string userId, CancellationToken ct)
    {
        return await context.JournalEntryList
            .Where(w => w.UserId == userId && w.Id == journalEntryId)
            .OrderByDescending(d => d.Created)
            .SingleOrDefaultAsync(ct);
    }

    public async Task<List<JournalEntryListEntity>> JournalEntryListGetByUserId(string userId, CancellationToken ct)
    {
        return await context.JournalEntryList
            .Where(w => w.UserId == userId)
            .OrderByDescending(d => d.Created)
            .ToListAsync(ct);
    }

    #endregion

    #region JournalTranscript

    public async Task JournalTranscriptCreate(JournalTranscriptEntity entity, CancellationToken ct)
    {
        await context.JournalTranscripts.AddAsync(entity, ct);
        await context.SaveChangesAsync(ct);
    }

    public async Task<JournalTranscriptEntity?> JournalTranscriptGetByJournalEntryFk(int journalEntryFk, CancellationToken ct)
    {
        return await context.JournalTranscripts
            .Where(w => w.JournalEntryFk == journalEntryFk && w.Transcript != null)
            .SingleOrDefaultAsync(ct);
    }

    #endregion

    #region Google

    public async Task<int> GoogleGetSpeech2TextSummary(DateTime start, CancellationToken ct)
    {
        return await context.JournalTranscripts
            .Where(w => w.Created >= start)
            .SumAsync(c => c.Cost, ct);
    }

    #endregion

    #region OpenAi

    public async Task<OpenAiCostSummaryModel> OpenApiGetChatbotSummary(DateTime start, CancellationToken ct)
    {
        var result = await context.JournalChatbots
            .Where(w => w.Created >= start)
            .GroupBy(g => 1)
            .Select(s => new OpenAiCostSummaryModel
            {
                InputTokens = s.Sum(s => s.InputTokens),
                OutputTokens = s.Sum(s => s.OutputTokens)
            })
            .SingleOrDefaultAsync(ct);

        return result ?? new OpenAiCostSummaryModel();
    }

    #endregion
}
