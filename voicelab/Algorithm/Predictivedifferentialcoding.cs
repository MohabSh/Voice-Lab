using System;
using System.Collections.Generic;
using voicelab.Interface;

public class PredictiveDifferentialCoding : IAudioCompressionAlgorithm
{
    public string Name => "Predictive Differential Coding";

    private int _quantizationLevels = 256; // افتراضي: 256 مستوى

    public void Configure(int quantizationLevels)
    {
        _quantizationLevels = quantizationLevels;
    }

    public byte[] Compress(short[] samples)
    {
        var data = new List<byte>();
        short prev1 = 0, prev2 = 0;

        int maxDiff = 65536;
        int stepSize = maxDiff / _quantizationLevels;
        int halfRange = maxDiff / 2;

        foreach (var sample in samples)
        {
            // التنبؤ الخطي: predicted = 2*prev1 - prev2
            short predicted = (short)(2 * prev1 - prev2);
            int diff = sample - predicted;

            // تكميم الفرق
            int quantizedIndex = (diff + halfRange) / stepSize;
            if (quantizedIndex >= _quantizationLevels) quantizedIndex = _quantizationLevels - 1;
            if (quantizedIndex < 0) quantizedIndex = 0;

            data.Add((byte)quantizedIndex);

            // إعادة بناء القيمة للتوقع التالي (باستخدام القيمة المفكوكة)
            int dequantizedDiff = (quantizedIndex * stepSize) - halfRange;
            short reconstructed = (short)(predicted + dequantizedDiff);

            // تحديث القيم السابقة
            prev2 = prev1;
            prev1 = reconstructed;
        }

        return data.ToArray();
    }

    public short[] Decompress(byte[] data)
    {
        var result = new List<short>();
        short prev1 = 0, prev2 = 0;

        int maxDiff = 65536;
        int stepSize = maxDiff / _quantizationLevels;
        int halfRange = maxDiff / 2;

        foreach (byte b in data)
        {
            int quantizedIndex = b;

            // التنبؤ الخطي
            short predicted = (short)(2 * prev1 - prev2);

            // فك تكميم الفرق
            int dequantizedDiff = (quantizedIndex * stepSize) - halfRange;
            short sample = (short)(predicted + dequantizedDiff);

            result.Add(sample);

            // تحديث القيم السابقة
            prev2 = prev1;
            prev1 = sample;
        }

        return result.ToArray();
    }
}