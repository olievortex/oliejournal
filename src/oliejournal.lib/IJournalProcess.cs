namespace oliejournal.lib;

public interface IJournalProcess
{
    Task IngestAudioEntry(string userId, Stream audio, CancellationToken ct);
}
