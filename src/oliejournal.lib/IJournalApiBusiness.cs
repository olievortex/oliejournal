using oliejournal.data.Entities;

namespace oliejournal.lib;

public interface IJournalApiBusiness
{
    Task<List<JournalEntryListEntity>> GetEntryList(string userId, CancellationToken ct);
    Task<JournalEntryListEntity?> GetEntry(int journalEntryId, string userId, CancellationToken ct);
}
