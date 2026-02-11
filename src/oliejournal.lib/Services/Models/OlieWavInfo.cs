namespace oliejournal.lib.Services.Models;

public class OlieWavInfo
{
    public TimeSpan Duration { get; set; }
    public int Channels { get; set; }
    public int SampleRate { get; set; }
    public int BitsPerSample { get; set; }
}
