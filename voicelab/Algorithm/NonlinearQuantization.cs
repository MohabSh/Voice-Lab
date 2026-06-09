using System;
using System.Collections.Generic;
using voicelab.Interface;

public class NonlinearQuantization : IAudioCompressionAlgorithm
{
    public string Name => "Nonlinear Quantization";

    public byte[] Compress(short[] samples)
    {
        List<byte> output = new();

        foreach (var s in samples)
        {
            double normalized = (s + 32768) / 65535.0;

            byte compressed = (byte)(Math.Pow(normalized, 0.5) * 255);
            output.Add(compressed);
        }

        return output.ToArray();
    }

    public short[] Decompress(byte[] data)
    {
        List<short> result = new();

        foreach (byte b in data)
        {
            double normalized = b / 255.0;
            double restored = Math.Pow(normalized, 2);
            short sample = (short)(restored * 65535 - 32768);
            result.Add(sample);
        }

        return result.ToArray();
    }
}