using System;
using System.Collections.Generic;
using voicelab.Interface;

public class DeltaModulation : IAudioCompressionAlgorithm
{
    public string Name => "Delta Modulation";

    private const int step = 500;

    public byte[] Compress(short[] samples)
    {
        List<byte> result = new();
        short prev = 0;
        byte currentByte = 0;
        int bitIndex = 0;

        foreach (var s in samples)
        {
            byte bit = (byte)(s > prev ? 1 : 0);

            currentByte |= (byte)(bit << (7 - bitIndex));
            bitIndex++;

            if (bitIndex == 8)
            {
                result.Add(currentByte);
                currentByte = 0;
                bitIndex = 0;
            }

            prev = (short)(prev + (bit == 1 ? step : -step));
        }

        if (bitIndex > 0)
        {
            result.Add(currentByte);
        }

        return result.ToArray();
    }

    public short[] Decompress(byte[] data)
    {
        List<short> result = new();
        short value = 0;

        foreach (var b in data)
        {
            for (int i = 0; i < 8; i++)
            {
                byte bit = (byte)((b >> (7 - i)) & 1);
                value = (short)(value + (bit == 1 ? step : -step));
                result.Add(value);
            }
        }

        return result.ToArray();
    }
}