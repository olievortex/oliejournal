using oliejournal.data.Entities;

namespace oliejournal.lib;

public interface IJournalApiBusiness
{
    Task<List<JournalEntryListEntity>> GetEntryList(string userId, CancellationToken ct);
}
