namespace oliejournal.lib.Exceptions;

public class RateLimitExceededException : Exception
{
    public RateLimitExceededException(string message) : base(message)
    {
    }
}