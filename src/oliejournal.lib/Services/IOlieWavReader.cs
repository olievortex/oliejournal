namespace oliejournal.lib.Services;

public interface IOlieWavReader
{
    OlieWavInfo GetOlieWavInfo(Stream stream);
}
