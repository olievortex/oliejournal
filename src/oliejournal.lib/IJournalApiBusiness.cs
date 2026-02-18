using oliejournal.lib.Models;

namespace oliejournal.lib;

public interface IJournalApiBusiness
{
    Task<List<JournalEntryListModel>> GetEntryList(string userId, CancellationToken ct);
    Task<JournalEntryListModel?> GetEntry(int journalEntryId, string userId, CancellationToken ct);
}
