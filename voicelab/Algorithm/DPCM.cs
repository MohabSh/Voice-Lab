using System;
using System.Collections.Generic;
using voicelab.Interface;

public class DPCM : IAudioCompressionAlgorithm
{
    public string Name => "DPCM";

    private int _quantizationLevels = 256; // افتراضي: 256 مستوى (8 بت)

    public void Configure(int quantizationLevels)
    {
        _quantizationLevels = quantizationLevels;
    }

    public byte[] Compress(short[] samples)
    {
        List<byte> data = new();
        short prev = 0;

        // حساب حجم الخطوة للتكميم
        int maxDiff = 65536; // من -32768 إلى 32767
        int stepSize = maxDiff / _quantizationLevels;
        int halfRange = maxDiff / 2;

        foreach (var sample in samples)
        {
            int diff = sample - prev;

            // تكميم الفرق
            int quantizedIndex = (diff + halfRange) / stepSize;
            if (quantizedIndex >= _quantizationLevels) quantizedIndex = _quantizationLevels - 1;
            if (quantizedIndex < 0) quantizedIndex = 0;

            // تخزين القيمة المكممة (0-255)
            data.Add((byte)quantizedIndex);

            // إعادة بناء القيمة للتوقع التالي
            int dequantizedDiff = (quantizedIndex * stepSize) - halfRange;
            prev = (short)(prev + dequantizedDiff);
        }

        return data.ToArray();
    }

    public short[] Decompress(byte[] data)
    {
        List<short> result = new();
        short prev = 0;

        int maxDiff = 65536;
        int stepSize = maxDiff / _quantizationLevels;
        int halfRange = maxDiff / 2;

        foreach (byte b in data)
        {
            int quantizedIndex = b;
            int dequantizedDiff = (quantizedIndex * stepSize) - halfRange;
            short sample = (short)(prev + dequantizedDiff);
            result.Add(sample);
            prev = sample;
        }

        return result.ToArray();
    }
}