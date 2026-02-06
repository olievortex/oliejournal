using Azure.Messaging.ServiceBus;

namespace oliejournal.lib;

public interface IJournalProcess
{
    Task IngestAudioEntry(string userId, Stream audio, ServiceBusSender sender, CancellationToken ct);
}
