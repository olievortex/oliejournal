using oliejournal.data.Entities;
using oliejournal.data.Models;

namespace oliejournal.data;

public interface IMyRepository
{
    #region ChatbotLogs

    Task ChatbotLogCreate(ChatbotLogEntity entity, CancellationToken ct);
    Task<ChatbotLogSummaryModel> ChatbotLogSummary(DateTime start, CancellationToken ct);

    #endregion

    #region Conversation

    Task ConversationCreate(ConversationEntity entity, CancellationToken ct);
    Task ConversationDelete(string id, CancellationToken ct);
    Task<List<ConversationEntity>> ConversationGetActiveList(string userId, CancellationToken ct);
    Task ConversationUpdate(ConversationEntity entity, CancellationToken ct);

    #endregion

    #region JournalEntry

    Task JournalEntryCreate(JournalEntryEntity entity, CancellationToken ct);
    Task<JournalEntryEntity?> JournalEntryGet(int id, CancellationToken ct);
    Task<JournalEntryEntity?> JournalEntryGetByHash(string userId, string hash, CancellationToken ct);
    Task JournalEntryUpdate(JournalEntryEntity entity, CancellationToken ct);
    Task JournalEntryDelete(int id, CancellationToken ct);

    #endregion

    #region JournalEntryList

    Task<JournalEntryListEntity?> JournalEntryListGetByUserId(int journalEntryId, string userId, CancellationToken ct);
    Task<List<JournalEntryListEntity>> JournalEntryListGetByUserId(string userId, CancellationToken ct);

    #endregion

    #region JournalTranscript

    Task JournalTranscriptCreate(JournalTranscriptEntity entity, CancellationToken ct);
    Task<JournalTranscriptEntity?> JournalTranscriptGetActiveByJournalEntryFk(int journalEntryFk, CancellationToken ct);
    Task<List<JournalTranscriptEntity>> JournalTranscriptGetByJournalEntryFk(int journalEntryFk, CancellationToken ct);
    Task JournalTranscriptDelete(int id, CancellationToken ct);

    #endregion

    #region Google

    Task<int> GoogleGetSpeech2TextSummary(DateTime start, CancellationToken ct);

    #endregion
}
