using System.Collections.Generic;
using voicelab.Interface;

public class AdaptiveDeltaModulation : IAudioCompressionAlgorithm
{
    public string Name => "Adaptive Delta Modulation";

    private const int MinStep = 100;
    private const int MaxStep = 8000;

    public byte[] Compress(short[] samples)
    {
        var result = new List<byte>();
        short prev = 0;
        int step = 500;
        byte currentByte = 0;
        int bitIndex = 0;

        foreach (var s in samples)
        {
            byte bit = (byte)(s >= prev ? 1 : 0);

            currentByte |= (byte)(bit << (7 - bitIndex));
            bitIndex++;

            if (bitIndex == 8)
            {
                result.Add(currentByte);
                currentByte = 0;
                bitIndex = 0;
            }

            prev = (short)(prev + (bit == 1 ? step : -step));

            // Adapt step size
            int diff = System.Math.Abs(s - prev);
            step = diff > step
                ? System.Math.Min(MaxStep, step * 2)
                : System.Math.Max(MinStep, step / 2);
        }

        if (bitIndex > 0)
            result.Add(currentByte);

        return result.ToArray();
    }

    public short[] Decompress(byte[] data)
    {
        var result = new List<short>();
        short value = 0;
        int step = 500;
        short prev = 0;

        foreach (var b in data)
        {
            for (int i = 0; i < 8; i++)
            {
                byte bit = (byte)((b >> (7 - i)) & 1);
                value = (short)(value + (bit == 1 ? step : -step));
                result.Add(value);

                int diff = System.Math.Abs(value - prev);
                step = diff > step
                    ? System.Math.Min(MaxStep, step * 2)
                    : System.Math.Max(MinStep, step / 2);

                prev = value;
            }
        }

        return result.ToArray();
    }
}