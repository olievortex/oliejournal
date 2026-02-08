namespace oliejournal.lib.Services.Models;

public class OlieTranscribeResult
{
    public string? Transcript { get; set; }
    public int Cost { get; set; }
    public Exception? Exception { get; set; }
    public int ServiceId { get; set; }
}
