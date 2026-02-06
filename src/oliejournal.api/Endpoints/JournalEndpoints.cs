using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using oliejournal.lib;
using System.Threading;
using System.Threading.Tasks;

namespace oliejournal.api.Endpoints;

public static class JournalEndpoints
{
    public static void MapJournalEndpoints(this WebApplication app)
    {
        app.MapPost("/api/journal/audioEntry", PostAudioEntry).DisableAntiforgery().RequireAuthorization();
    }

    public static async Task<Ok<int>> PostAudioEntry(IFormFile file, IJournalProcess process, CancellationToken ct)
    {
        using var stream = file.OpenReadStream();
        await process.IngestAudioEntry(stream, ct);
        return TypedResults.Ok(42);
    }
}
