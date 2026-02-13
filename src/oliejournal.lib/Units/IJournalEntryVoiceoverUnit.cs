using oliejournal.data.Entities;
using oliejournal.lib.Services.Models;
using System.Diagnostics;

namespace oliejournal.lib.Units;

public interface IJournalEntryVoiceoverUnit
{
    Task<OlieWavInfo> GetWavInfo(byte[] bytes);
    Task<JournalChatbotEntity> GetChatbotEntryOrThrow(int journalEntryId, CancellationToken ct);
    Task<string> SaveLocalFile(byte[] bytes, CancellationToken ct);
    Task UpdateEntry(string blobPath, int length, Stopwatch stopwatch, OlieWavInfo wavInfo, JournalEntryEntity entry, CancellationToken ct);
    Task<byte[]> VoiceOver(string script, CancellationToken ct);
}
