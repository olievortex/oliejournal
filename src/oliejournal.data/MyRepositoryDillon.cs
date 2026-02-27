using oliejournal.data.Entities;
using System.Diagnostics.CodeAnalysis;

namespace oliejournal.data;

[ExcludeFromCodeCoverage]
public class MyRepositoryDillon(MyContext context) : MyRepository(context)
{
    private readonly Random random = new();

    #region ChatbotConversation

    public override async Task ChatbotConversationCreate(ChatbotConversationEntity entity, CancellationToken ct)
    {
        await Task.Delay(random.Next(100, 500), ct);
    }

    public override async Task ChatbotConversationDelete(string id, CancellationToken ct)
    {
        await Task.Delay(random.Next(100, 500), ct);
    }

    public override async Task ChatbotConversationUpdate(ChatbotConversationEntity entity, CancellationToken ct)
    {
        await Task.Delay(random.Next(100, 500), ct);
    }

    #endregion

    #region ChatbotLogs

    public override async Task ChatbotLogCreate(ChatbotLogEntity entity, CancellationToken ct)
    {
        await Task.Delay(random.Next(100, 500), ct);
    }

    #endregion

    #region JournalEntry

    public override async Task JournalEntryCreate(JournalEntryEntity entity, CancellationToken ct)
    {
        await Task.Delay(random.Next(100, 500), ct);
    }

    public override async Task JournalEntryUpdate(JournalEntryEntity entity, CancellationToken ct)
    {
        await Task.Delay(random.Next(100, 500), ct);
    }

    public override async Task JournalEntryDelete(int id, CancellationToken ct)
    {
        await Task.Delay(random.Next(100, 500), ct);
    }

    #endregion

    #region TranscriptLogs

    public override async Task TranscriptLogCreate(TranscriptLogEntity entity, CancellationToken ct)
    {
        await Task.Delay(random.Next(100, 500), ct);
    }

    #endregion
}
