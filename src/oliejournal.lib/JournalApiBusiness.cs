using oliejournal.data;
using oliejournal.data.Entities;

namespace oliejournal.lib;

public class JournalApiBusiness(IMyRepository repo) : IJournalApiBusiness
{
    public async Task<List<JournalEntryListEntity>> GetEntryList(string userId, CancellationToken ct)
    {
        return await repo.JournalEntryListGetByUserId(userId, ct);
    }
}
