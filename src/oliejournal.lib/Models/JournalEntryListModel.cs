using oliejournal.data.Entities;

namespace oliejournal.lib.Models;

public class JournalEntryListModel
{
    public int Id { get; init; }
    public string UserId { get; init; } = string.Empty;
    public DateTime Created { get; init; }
    public string? Transcript { get; init; }
    public string? ResponsePath { get; init; }
    public string? ResponseText { get; init; }
    public float? Latitude { get; init; }
    public float? Longitude { get; init; }

    public static JournalEntryListModel FromEntity(JournalEntryListEntity entity)
    {
        return new JournalEntryListModel
        {
            Id = entity.Id,
            UserId = entity.UserId,
            Created = entity.Created.AsUtc(),
            Transcript = entity.Transcript,
            ResponsePath = entity.ResponsePath,
            ResponseText = entity.ResponseText,
            Latitude = entity.Latitude,
            Longitude = entity.Longitude,
        };
    }
}
