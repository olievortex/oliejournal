using oliejournal.data;
using oliejournal.data.Entities;

namespace oliejournal.lib;

public class JournalApiBusiness(IMyRepository repo) : IJournalApiBusiness
{
    public async Task<List<JournalEntryListEntity>> GetEntryList(string userId, CancellationToken ct)
    {
        return await repo.JournalEntryListGetByUserId(userId, ct);
    }

    public async Task<int> GetEntryStatus(int journalEntryId, string userId, CancellationToken ct)
    {
        var result = await repo.JournalEntryListGetByUserId(journalEntryId, userId, ct);
        if (result == null) { return 0; }

        if (result.Transcript == null) { return 1; }
        if (result.ResponsePath == null) { return 2; }
        return 3;
    }
}
