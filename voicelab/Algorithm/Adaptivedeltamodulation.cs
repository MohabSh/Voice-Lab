using System.Collections.Generic;
using voicelab.Interface;

public class AdaptiveDeltaModulation : IAudioCompressionAlgorithm
{
    public string Name => "Adaptive Delta Modulation";

    private int _quantizationLevels = 2; // افتراضي: 2 مستويات (بت واحد)

    public void Configure(int quantizationLevels)
    {
        _quantizationLevels = quantizationLevels;
    }

    private const int MinStep = 100;
    private const int MaxStep = 8000;

    public byte[] Compress(short[] samples)
    {
        var result = new List<byte>();
        short prev = 0;
        int step = 500;
        byte currentByte = 0;
        int bitIndex = 0;

        // حساب عدد البتات المطلوبة لكل عينة بناءً على مستويات التكميم
        int bitsPerSample = (int)System.Math.Log2(_quantizationLevels);
        int samplesPerByte = 8 / bitsPerSample;

        foreach (var s in samples)
        {
            // تكميم الفرق بناءً على عدد المستويات
            int diff = s - prev;
            int maxDiff = 32768;
            int stepSize = maxDiff / _quantizationLevels;
            int quantizedIndex = (diff + maxDiff) / stepSize;
            if (quantizedIndex >= _quantizationLevels) quantizedIndex = _quantizationLevels - 1;
            if (quantizedIndex < 0) quantizedIndex = 0;

            // تخزين القيمة المكممة
            currentByte |= (byte)(quantizedIndex << (8 - bitsPerSample - (bitIndex * bitsPerSample)));
            bitIndex++;

            if (bitIndex == samplesPerByte)
            {
                result.Add(currentByte);
                currentByte = 0;
                bitIndex = 0;
            }

            // إعادة بناء القيمة
            int dequantizedValue = (quantizedIndex * stepSize) - maxDiff;
            prev = (short)(prev + dequantizedValue);

            // تكييف حجم الخطوة
            int actualDiff = System.Math.Abs(s - prev);
            step = actualDiff > step
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

        int bitsPerSample = (int)System.Math.Log2(_quantizationLevels);
        int samplesPerByte = 8 / bitsPerSample;
        int maxDiff = 32768;
        int stepSize = maxDiff / _quantizationLevels;

        foreach (var b in data)
        {
            for (int i = 0; i < samplesPerByte; i++)
            {
                int shift = 8 - bitsPerSample - (i * bitsPerSample);
                int quantizedIndex = (b >> shift) & ((1 << bitsPerSample) - 1);

                int dequantizedValue = (quantizedIndex * stepSize) - maxDiff;
                value = (short)(value + dequantizedValue);
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