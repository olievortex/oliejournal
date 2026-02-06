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
}
