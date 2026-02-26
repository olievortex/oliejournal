using Microsoft.EntityFrameworkCore;
using oliejournal.data.Entities;
using oliejournal.data.Models;
using System.Diagnostics.CodeAnalysis;

namespace oliejournal.data;

[ExcludeFromCodeCoverage]
public class MyRepository(MyContext context) : IMyRepository
{
    #region ChatbotConversation

    public async Task ChatbotConversationCreate(ChatbotConversationEntity entity, CancellationToken ct)
    {
        await context.ChatbotConversations.AddAsync(entity, ct);
        await context.SaveChangesAsync(ct);
    }

    public async Task ChatbotConversationDelete(string id, CancellationToken ct)
    {
        var entity = await context.ChatbotConversations.FindAsync([id], ct);
        if (entity is not null)
        {
            context.ChatbotConversations.Remove(entity);
            await context.SaveChangesAsync(ct);
        }
    }

    public async Task<List<ChatbotConversationEntity>> ChatbotConversationGetActiveList(string userId, CancellationToken ct)
    {
        return await context.ChatbotConversations
            .Where(w => w.UserId == userId)
            .OrderBy(o => o.Timestamp)
            .ToListAsync(ct);
    }

    public async Task ChatbotConversationUpdate(ChatbotConversationEntity entity, CancellationToken ct)
    {
        context.ChatbotConversations.Update(entity);
        await context.SaveChangesAsync(ct);
    }

    #endregion

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

    #region TranscriptLogs

    public async Task TranscriptLogCreate(TranscriptLogEntity entity, CancellationToken ct)
    {
        await context.TranscriptLogs.AddAsync(entity, ct);
        await context.SaveChangesAsync(ct);
    }

    public async Task<int> TranscriptLogSummary(DateTime start, CancellationToken ct)
    {
        return await context.TranscriptLogs
            .Where(w => w.Created >= start)
            .SumAsync(c => c.Cost, ct);
    }

    #endregion
}
