using oliejournal.data;
using oliejournal.lib.Models;

namespace oliejournal.lib;

public class JournalApiBusiness(IMyRepository repo) : IJournalApiBusiness
{
    public async Task<List<JournalEntryListModel>> GetEntryList(string userId, CancellationToken ct)
    {
        return [.. (await repo.JournalEntryGetListByUserId(userId, ct)).Select(JournalEntryListModel.FromEntity)];
    }

    public async Task<JournalEntryListModel?> GetEntry(int journalEntryId, string userId, CancellationToken ct)
    {
        var result = await repo.JournalEntryGetByUserId(journalEntryId, userId, ct);

        return result is null ? null : JournalEntryListModel.FromEntity(result);
    }
}
