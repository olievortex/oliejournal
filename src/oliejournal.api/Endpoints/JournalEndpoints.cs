using Microsoft.AspNetCore.Http.HttpResults;
using oliejournal.api.Models;
using oliejournal.data.Entities;
using oliejournal.lib;
using oliejournal.lib.Processes.JournalProcess;
using oliejournal.lib.Services;
using System.Security.Claims;

namespace oliejournal.api.Endpoints;

public static class JournalEndpoints
{
    public static void MapJournalEndpoints(this WebApplication app)
    {
        app.MapGet("/api/journal/entries", GetEntryList).RequireAuthorization();
        app.MapGet("/api/journal/entryStatus/{id}", GetEntryStatus).RequireAuthorization();
        app.MapPost("/api/journal/audioEntry", PostAudioEntry).DisableAntiforgery().RequireAuthorization();
    }

    public static async Task<Results<Ok<IntResultModel>, UnauthorizedHttpResult>> GetEntryStatus(int id, ClaimsPrincipal user, IJournalApiBusiness business, CancellationToken ct)
    {
        var userId = user.Identity?.Name;
        if (userId is null) return TypedResults.Unauthorized();

        return TypedResults.Ok(new IntResultModel { Id = await business.GetEntryStatus(id, userId, ct) });
    }

    public static async Task<Results<Ok<List<JournalEntryListEntity>>, UnauthorizedHttpResult>> GetEntryList(ClaimsPrincipal user, IJournalApiBusiness business, CancellationToken ct)
    {
        var userId = user.Identity?.Name;
        if (userId is null) return TypedResults.Unauthorized();

        return TypedResults.Ok(await business.GetEntryList(userId, ct));
    }

    public static async Task<Results<Ok<IntResultModel>, UnauthorizedHttpResult>> PostAudioEntry(IFormFile file, ClaimsPrincipal user, IJournalProcess process, IOlieConfig config, CancellationToken ct)
    {
        var userId = user.Identity?.Name;
        if (userId is null) return TypedResults.Unauthorized();

        using var stream = file.OpenReadStream();
        var sender = config.ServiceBusSender();
        var client = config.BlobContainerClient();

        var id = await process.Ingest(userId, stream, sender, client, ct);

        return TypedResults.Ok(new IntResultModel { Id = id });
    }
}
