using oliejournal.data.Entities;
using oliejournal.lib.Services.Models;

namespace oliejournal.lib.Processes.JournalProcess;

public interface IJournalEntryVoiceoverUnit
{
    void DeleteLocalFile(JournalEntryEntity entry);
    Task<OlieWavInfo> GetWavInfo(byte[] bytes);
    Task<string> SaveLocalFile(byte[] bytes, CancellationToken ct);
    Task UpdateEntry(string blobPath, int length, OlieWavInfo wavInfo, JournalEntryEntity entry, CancellationToken ct);
    Task<byte[]> VoiceOver(string script, CancellationToken ct);
}
