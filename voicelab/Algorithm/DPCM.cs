using System;
using System.Collections.Generic;
using voicelab.Interface;

public class DPCM : IAudioCompressionAlgorithm
{
    public string Name => "DPCM";

    public byte[] Compress(short[] samples)
    {
        List<byte> data = new();
        short prev = 0;

        foreach (var sample in samples)
        {
            short diff = (short)(sample - prev);
            if (diff > 127) diff = 127;
            if (diff < -128) diff = -128;

            data.Add((byte)diff);
            prev = sample;
        }

        return data.ToArray();
    }

    public short[] Decompress(byte[] data)
    {
        List<short> result = new();
        short prev = 0;

        foreach (byte b in data)
        {
            sbyte diff = (sbyte)b;
            short sample = (short)(prev + diff);
            result.Add(sample);
            prev = sample;
        }

        return result.ToArray();
    }
}