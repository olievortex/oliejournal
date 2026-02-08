using oliejournal.data.Entities;

namespace oliejournal.data;

public interface IMyRepository
{
    #region JournalEntry

    Task JournalEntryCreate(JournalEntryEntity entity, CancellationToken ct);

    Task<JournalEntryEntity?> JournalEntryGet(int id, CancellationToken ct);

    Task JournalEntryUpdate(JournalEntryEntity entity, CancellationToken ct);

    #endregion

    #region JournalTranscript

    Task JournalTranscriptCreate(JournalTranscriptEntity entity, CancellationToken ct);

    Task<JournalTranscriptEntity?> JournalTranscriptGetByJournalEntryFk(int journalEntryFk, CancellationToken ct);

    #endregion

    #region Google

    Task<int> GoogleGetSpeech2TextSummary(DateTime start, CancellationToken ct);

    #endregion
}
