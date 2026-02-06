using oliejournal.data.Entities;

namespace oliejournal.data;

public interface IMyRepository
{
    #region JournalEntry

    Task JournalEntryCreate(JournalEntryEntity entity, CancellationToken ct);

    #endregion

}
