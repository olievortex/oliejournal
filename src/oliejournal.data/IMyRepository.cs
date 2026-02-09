using oliejournal.data.Entities;
using oliejournal.data.Models;

namespace oliejournal.data;

public interface IMyRepository
{
    #region Conversation

    Task ConversationCreate(ConversationEntity entity, CancellationToken ct);
    Task<List<ConversationEntity>> ConversationGetActiveList(string userId, CancellationToken ct);
    Task ConversationUpdate(ConversationEntity entity, CancellationToken ct);

    #endregion

    #region JournalChatbot

    Task JournalChatbotCreate(JournalChatbotEntity entity, CancellationToken ct);
    Task<JournalChatbotEntity?> JournalChatbotGetByJournalTranscriptFk(int journalTranscriptFk, CancellationToken ct);

    #endregion

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

    #region OpenAi

    Task<OpenAiCostSummary> OpenApiGetChatbotSummary(DateTime start, CancellationToken ct);

    #endregion
}
