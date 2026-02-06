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

    #endregion
}
