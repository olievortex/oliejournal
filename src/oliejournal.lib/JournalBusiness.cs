using oliejournal.lib.Services;

namespace oliejournal.lib;

public class JournalBusiness(IOlieWavReader owr) : IJournalBusiness
{
    public void EnsureAudioValidates(byte[] file)
    {
        if (file.Length == 0) throw new ApplicationException("WAV file empty");
        if (file.Length > 9 * 1024 * 1024) throw new ApplicationException($"WAV file {file.Length} > 9MB");

        using var stream = new MemoryStream(file);
        var info = owr.GetOlieWavInfo(stream);

        if (info.Channels > 2) throw new ApplicationException($"WAV file has {info.Channels} channels");
        if (info.SampleRate < 8000 || info.SampleRate > 48000) throw new ApplicationException($"WAV file has {info.SampleRate} sample rate");
        if (info.BitsPerSample > 24) throw new ApplicationException($"WAV file has {info.BitsPerSample} bits per sample");
        if (info.Duration > TimeSpan.FromSeconds(55)) throw new ApplicationException($"WAV file duration is {info.Duration.TotalSeconds}");
    }
}
