using oliejournal.lib.Services;

namespace oliejournal.lib;

public interface IJournalBusiness
{
    Task CreateJournalEntry(string userId, OlieWavInfo olieWavInfo, string path, int length, CancellationToken ct);
    OlieWavInfo EnsureAudioValidates(byte[] file);
}
