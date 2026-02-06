namespace oliejournal.lib.Services;

public interface IOlieService
{
    Task<byte[]> ToByteArray(Stream stream, CancellationToken ct);
}
