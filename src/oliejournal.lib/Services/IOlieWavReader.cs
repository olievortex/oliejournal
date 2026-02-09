using oliejournal.lib.Services.Models;

namespace oliejournal.lib.Services;

public interface IOlieWavReader
{
    OlieWavInfo GetOlieWavInfo(byte[] data);
    OlieWavInfo GetOlieWavInfo(string path);
    OlieWavInfo GetOlieWavInfo(Stream stream);
}
