using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using oliejournal.api.Models;
using oliejournal.lib;
using oliejournal.lib.Models;
using oliejournal.lib.Processes.JournalProcess;
using oliejournal.lib.Services;
using System.Security.Claims;

namespace oliejournal.api.Endpoints;

public static class JournalEndpoints
{
    public static void MapJournalEndpoints(this WebApplication app)
    {
        app.MapGet("/api/journal/entries", GetEntryList).RequireAuthorization();
        app.MapGet("/api/journal/entries/{id}", GetEntry).RequireAuthorization();
        app.MapPost("/api/journal/audioEntry", PostAudioEntry).DisableAntiforgery().RequireAuthorization();
        app.MapDelete("/api/journal/entries/{id}", DeleteEntry).RequireAuthorization();
    }

    public static async Task<Results<Ok<JournalEntryListModel>, NotFound, UnauthorizedHttpResult>> GetEntry(int id, ClaimsPrincipal user, IJournalApiBusiness business, CancellationToken ct)
    {
        var userId = user.Identity?.Name;
        if (userId is null) return TypedResults.Unauthorized();

        var entry = await business.GetEntry(id, userId, ct);
        if (entry is null) return TypedResults.NotFound();

        return TypedResults.Ok(entry);
    }

    public static async Task<Results<Ok<List<JournalEntryListModel>>, UnauthorizedHttpResult>> GetEntryList(ClaimsPrincipal user, IJournalApiBusiness business, CancellationToken ct)
    {
        var userId = user.Identity?.Name;
        if (userId is null) return TypedResults.Unauthorized();

        return TypedResults.Ok(await business.GetEntryList(userId, ct));
    }

    public static async Task<Results<Ok<IntResultModel>, UnauthorizedHttpResult>> PostAudioEntry(IFormFile file, [FromForm] string? latitude, [FromForm] string? longitude, ClaimsPrincipal user, IJournalProcess process, IOlieConfig config, CancellationToken ct)
    {
        var userId = user.Identity?.Name;
        if (userId is null) return TypedResults.Unauthorized();

        using var stream = file.OpenReadStream();
        var sender = config.ServiceBusSender();
        var client = config.BlobContainerClient();

        var lat = latitude.SafeFloat();
        var lon = longitude.SafeFloat();

        var id = await process.Ingest(userId, stream, lat, lon, sender, client, ct);

        return TypedResults.Ok(new IntResultModel { Id = id });
    }

    public static async Task<Results<NoContent, NotFound, UnauthorizedHttpResult>> DeleteEntry(int id, ClaimsPrincipal user, IJournalProcess process, CancellationToken ct)
    {
        var userId = user.Identity?.Name;
        if (userId is null) return TypedResults.Unauthorized();

        var deleted = await process.DeleteEntry(id, userId, ct);
        if (!deleted) return TypedResults.NotFound();

        return TypedResults.NoContent();
    }
}
