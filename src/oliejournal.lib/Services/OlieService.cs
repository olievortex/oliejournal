using System.Diagnostics.CodeAnalysis;

namespace oliejournal.lib.Services;

[ExcludeFromCodeCoverage]
public class OlieService : IOlieService
{
    public async Task<byte[]> ToByteArray(Stream stream, CancellationToken ct)
    {
        using var result = new MemoryStream();
        await stream.CopyToAsync(result, ct);

        return result.ToArray();
    }
}
