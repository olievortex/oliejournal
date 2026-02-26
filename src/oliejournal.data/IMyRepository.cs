using oliejournal.data.Entities;
using oliejournal.data.Models;

namespace oliejournal.data;

public interface IMyRepository
{
    #region ChatbotConversations

    Task ChatbotConversationCreate(ChatbotConversationEntity entity, CancellationToken ct);
    Task ChatbotConversationDelete(string id, CancellationToken ct);
    Task<List<ChatbotConversationEntity>> ChatbotConversationGetActiveList(string userId, CancellationToken ct);
    Task ChatbotConversationUpdate(ChatbotConversationEntity entity, CancellationToken ct);

    #endregion

    #region ChatbotLogs

    Task ChatbotLogCreate(ChatbotLogEntity entity, CancellationToken ct);
    Task<ChatbotLogSummaryModel> ChatbotLogSummary(DateTime start, CancellationToken ct);

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

    #region TranscriptLogs

    Task TranscriptLogCreate(TranscriptLogEntity entity, CancellationToken ct);
    Task<int> TranscriptLogSummary(DateTime start, CancellationToken ct);

    #endregion
}
