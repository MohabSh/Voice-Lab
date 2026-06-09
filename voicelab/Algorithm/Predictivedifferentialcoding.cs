using System.Collections.Generic;
using voicelab.Interface;

public class PredictiveDifferentialCoding : IAudioCompressionAlgorithm
{
    public string Name => "Predictive Differential Coding";

    public byte[] Compress(short[] samples)
    {
        var data = new List<byte>();
        short prev1 = 0, prev2 = 0;

        foreach (var sample in samples)
        {
            // Linear prediction: predicted = 2*prev1 - prev2
            short predicted = (short)(2 * prev1 - prev2);
            short diff = (short)(sample - predicted);

            if (diff > 127) diff = 127;
            if (diff < -128) diff = -128;

            data.Add((byte)(sbyte)diff);

            prev2 = prev1;
            prev1 = sample;
        }

        return data.ToArray();
    }

    public short[] Decompress(byte[] data)
    {
        var result = new List<short>();
        short prev1 = 0, prev2 = 0;

        foreach (byte b in data)
        {
            sbyte diff = (sbyte)b;
            short predicted = (short)(2 * prev1 - prev2);
            short sample = (short)(predicted + diff);

            result.Add(sample);
            prev2 = prev1;
            prev1 = sample;
        }

        return result.ToArray();
    }
}