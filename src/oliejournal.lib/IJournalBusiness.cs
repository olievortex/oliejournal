using Azure.Messaging.ServiceBus;
using oliejournal.data.Entities;
using oliejournal.lib.Enums;
using oliejournal.lib.Services;

namespace oliejournal.lib;

public interface IJournalBusiness
{
    Task<JournalEntryEntity> CreateJournalEntry(string userId, OlieWavInfo olieWavInfo, string path, int length, CancellationToken ct);
    Task CreateJournalMessage(int id, AudioProcessStepEnum step, ServiceBusSender sender, CancellationToken ct);
    OlieWavInfo EnsureAudioValidates(byte[] file);
}
