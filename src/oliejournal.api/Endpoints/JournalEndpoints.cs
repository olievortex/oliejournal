using Microsoft.AspNetCore.Http.HttpResults;
using oliejournal.lib;
using oliejournal.lib.Services;
using System.Security.Claims;

namespace oliejournal.api.Endpoints;

public static class JournalEndpoints
{
    public static void MapJournalEndpoints(this WebApplication app)
    {
        app.MapPost("/api/journal/audioEntry", PostAudioEntry).DisableAntiforgery().RequireAuthorization();
    }

    public static async Task<Results<Ok<int>, UnauthorizedHttpResult>> PostAudioEntry(IFormFile file, ClaimsPrincipal user, IJournalProcess process, IOlieConfig config, CancellationToken ct)
    {
        var userId = user.Identity?.Name;
        if (userId is null) return TypedResults.Unauthorized();

        using var stream = file.OpenReadStream();
        var sender = config.ServiceBusSender();
        
        await process.IngestAudioEntry(userId, stream, sender, ct);
        
        return TypedResults.Ok(42);
    }
}
