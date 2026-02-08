using Microsoft.EntityFrameworkCore;
using oliejournal.data.Entities;

namespace oliejournal.data;

public class MyRepository(MyContext context) : IMyRepository
{
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

    public async Task JournalEntryUpdate(JournalEntryEntity entity, CancellationToken ct)
    {
        context.JournalEntries.Update(entity);
        await context.SaveChangesAsync(ct);
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
}
