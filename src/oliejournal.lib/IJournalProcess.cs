namespace oliejournal.lib;

public interface IJournalProcess
{
    Task IngestAudioEntry(Stream audio, CancellationToken ct);
}
