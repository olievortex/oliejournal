using oliejournal.lib.Services;

namespace oliejournal.lib;

public class JournalProcess(IJournalBusiness business, IOlieService os) : IJournalProcess
{
    public async Task IngestAudioEntry(string userId, Stream audio, CancellationToken ct)
    {
        var file = await os.ToByteArray(audio, ct);

        var wavInfo = business.EnsureAudioValidates(file);
        await business.CreateJournalEntry(userId, wavInfo, "dillon.wav", file.Length, ct);
    }
}
