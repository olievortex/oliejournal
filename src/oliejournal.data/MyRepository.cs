using Microsoft.EntityFrameworkCore;
using oliejournal.data.Entities;
using oliejournal.data.Models;
using System.Diagnostics.CodeAnalysis;

namespace oliejournal.data;

[ExcludeFromCodeCoverage]
public class MyRepository(MyContext context) : IMyRepository
{
    #region ChatbotLogs

    public async Task ChatbotLogCreate(ChatbotLogEntity entity, CancellationToken ct)
    {
        await context.ChatbotLogs.AddAsync(entity, ct);
        await context.SaveChangesAsync(ct);
    }

    public async Task<ChatbotLogSummaryModel> ChatbotLogSummary(DateTime start, CancellationToken ct)
    {
        var result = await context.ChatbotLogs
            .Where(w => w.Created >= start)
            .GroupBy(g => 1)
            .Select(s => new ChatbotLogSummaryModel
            {
                InputTokens = s.Sum(s => s.InputTokens),
                OutputTokens = s.Sum(s => s.OutputTokens)
            })
            .SingleOrDefaultAsync(ct);

        return result ?? new ChatbotLogSummaryModel();
    }

    #endregion

    #region Conversation

    public async Task ConversationCreate(ConversationEntity entity, CancellationToken ct)
    {
        await context.Conversations.AddAsync(entity, ct);
        await context.SaveChangesAsync(ct);
    }

    public async Task ConversationDelete(string id, CancellationToken ct)
    {
        var entity = await context.Conversations.FindAsync([id], ct);
        if (entity is not null)
        {
            context.Conversations.Remove(entity);
            await context.SaveChangesAsync(ct);
        }
    }

    public async Task<List<ConversationEntity>> ConversationGetActiveList(string userId, CancellationToken ct)
    {
        return await context.Conversations
            .Where(w => w.UserId == userId)
            .OrderBy(o => o.Timestamp)
            .ToListAsync(ct);
    }

    public async Task ConversationUpdate(ConversationEntity entity, CancellationToken ct)
    {
        context.Conversations.Update(entity);
        await context.SaveChangesAsync(ct);
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

    public async Task JournalEntryDelete(int id, CancellationToken ct)
    {
        var entity = await context.JournalEntries.FindAsync([id], ct);
        if (entity is not null)
        {
            context.JournalEntries.Remove(entity);
            await context.SaveChangesAsync(ct);
        }
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

    public async Task<JournalTranscriptEntity?> JournalTranscriptGetActiveByJournalEntryFk(int journalEntryFk, CancellationToken ct)
    {
        return await context.JournalTranscripts
            .Where(w => w.JournalEntryFk == journalEntryFk && w.Transcript != null)
            .SingleOrDefaultAsync(ct);
    }

    public async Task<List<JournalTranscriptEntity>> JournalTranscriptGetByJournalEntryFk(int journalEntryFk, CancellationToken ct)
    {
        return await context.JournalTranscripts
            .Where(w => w.JournalEntryFk == journalEntryFk)
            .ToListAsync(ct);
    }

    public async Task JournalTranscriptDelete(int id, CancellationToken ct)
    {
        var entity = await context.JournalTranscripts.FindAsync([id], ct);
        if (entity is not null)
        {
            context.JournalTranscripts.Remove(entity);
            await context.SaveChangesAsync(ct);
        }
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
}
