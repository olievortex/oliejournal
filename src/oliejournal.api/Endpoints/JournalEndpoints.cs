using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using oliejournal.lib;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace oliejournal.api.Endpoints;

public static class JournalEndpoints
{
    public static void MapJournalEndpoints(this WebApplication app)
    {
        app.MapPost("/api/journal/audioEntry", PostAudioEntry).DisableAntiforgery().RequireAuthorization();
    }

    public static async Task<Results<Ok<int>, UnauthorizedHttpResult>> PostAudioEntry(IFormFile file, ClaimsPrincipal user, IJournalProcess process, CancellationToken ct)
    {
        var userId = user.Identity?.Name;
        if (userId is null) return TypedResults.Unauthorized();

        using var stream = file.OpenReadStream();
        await process.IngestAudioEntry(userId, stream, ct);
        return TypedResults.Ok(42);
    }
}
