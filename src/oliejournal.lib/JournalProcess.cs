using oliejournal.lib.Services;

namespace oliejournal.lib;

public class JournalProcess(IJournalBusiness business, IOlieService os) : IJournalProcess
{
    public async Task IngestAudioEntry(Stream audio, CancellationToken ct)
    {
        var file = await os.ToByteArray(audio, ct);

        business.EnsureAudioValidates(file);
    }
}
