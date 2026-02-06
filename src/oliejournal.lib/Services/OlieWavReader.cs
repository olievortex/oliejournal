using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace oliejournal.lib.Services;

public class OlieWavInfo
{
    public TimeSpan Duration { get; set; }
    public int Channels { get; set; }
    public int SampleRate { get; set; }
    public int BitsPerSample { get; set; }
}

[ExcludeFromCodeCoverage]
public class OlieWavReader : IOlieWavReader
{
    // http://soundfile.sapp.org/doc/WaveFormat/
    private class Riff
    {
        public static Riff Read(BinaryReader br)
        {
            var result = new Riff
            {
                ChunkId = ReadString(br, 4),
                ChunkSize = br.ReadUInt32(),
                Format = ReadString(br, 4)
            };

            if (result.ChunkId != "RIFF") throw new ApplicationException("Missing RIFF header");
            if (result.Format != "WAVE") throw new ApplicationException("Not a WAVE file");

            return result;
        }

        private string ChunkId { get; init; } = "RIFF";
        private long ChunkSize { get; set; }
        private string Format { get; init; } = "WAVE";
    }

    private class Subchunk
    {
        public static Subchunk Read(BinaryReader br)
        {
            var result = new Subchunk
            {
                SubchunkId = ReadString(br, 4),
                SubchunkSize = br.ReadUInt32()
            };

            return result;
        }

        public string SubchunkId { get; private init; } = string.Empty;
        public long SubchunkSize { get; private init; }
    }

    private class Fmt
    {
        public static Fmt Read(Subchunk subchunk, BinaryReader br)
        {
            var result = new Fmt
            {
                SubchunkId = subchunk.SubchunkId,
                SubchunkSize = subchunk.SubchunkSize,
                AudioFormat = br.ReadInt16(),
                NumChannels = br.ReadInt16(),
                SampleRate = br.ReadInt32(),
                ByteRate = br.ReadInt32(),
                BlockAlign = br.ReadInt16(),
                BitsPerSample = br.ReadInt16()
            };

            if (result.SubchunkId != "fmt ") throw new ApplicationException("Missing fmt header");
            if (result.AudioFormat != 1) throw new ApplicationException("Expected PCM format");
            SkipGarbage(result.SubchunkSize - 16, br);

            return result;
        }

        private string SubchunkId { get; init; } = string.Empty;
        private long SubchunkSize { get; init; }
        private int AudioFormat { get; init; }
        public int NumChannels { get; init; }
        public int SampleRate { get; init; }
        public int ByteRate { get; private init; }
        private int BlockAlign { get; init; }
        public int BitsPerSample { get; init; }
    }

    private class Junk
    {
        public static Junk Read(Subchunk subchunk, BinaryReader br)
        {
            var result = new Junk
            {
                SubchunkSize = subchunk.SubchunkSize,
            };

            SkipGarbage(result.SubchunkSize, br);

            return result;
        }

        public long SubchunkSize { get; private init; }
    }

    private static void SkipGarbage(long length, BinaryReader br)
    {
        var bytesLeft = length;

        while (bytesLeft > 0)
        {
            var readLength = bytesLeft > 5242880 ? 5242880 : (int)bytesLeft;
            var buffer = br.ReadBytes(readLength);

            bytesLeft -= buffer.Length;
        }
    }

    private static string ReadString(BinaryReader br, int length)
    {
        var bytes = br.ReadBytes(length);
        var value = Encoding.ASCII.GetString(bytes);

        return value;
    }

    public OlieWavInfo GetOlieWavInfo(byte[] data)
    {
        using var s = new MemoryStream(data);

        return GetOlieWavInfo(s);
    }

    public OlieWavInfo GetOlieWavInfo(string path)
    {
        using var s = File.OpenRead(path);

        return GetOlieWavInfo(s);
    }

    public OlieWavInfo GetOlieWavInfo(Stream stream)
    {
        using var br = new BinaryReader(stream, Encoding.Default, true);

        _ = Riff.Read(br);
        Fmt? fmt = null;
        Junk? data = null;

        while (data is null || fmt is null)
        {
            var subchunk = Subchunk.Read(br);

            switch (subchunk.SubchunkId)
            {
                case "data":
                    data = Junk.Read(subchunk, br);
                    break;
                case "fmt ":
                    fmt = Fmt.Read(subchunk, br);
                    break;
                case "JUNK":
                    Junk.Read(subchunk, br);
                    break;
                default:
                    throw new InvalidOperationException($"Don't know what to do with a {subchunk.SubchunkId} chunk");
            }
        }

        return new OlieWavInfo
        {
            Duration = TimeSpan.FromSeconds(data.SubchunkSize / fmt.ByteRate + 1),
            SampleRate = fmt.SampleRate,
            Channels = fmt.NumChannels,
            BitsPerSample = fmt.BitsPerSample
        };
    }
}
