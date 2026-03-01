namespace oliejournal.lib.Services;

public interface IOlieKinde
{
    Task<bool> DeleteUser(string userId, CancellationToken ct);
}