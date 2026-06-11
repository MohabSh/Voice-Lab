using System;
using System.Collections.Generic;
using voicelab.Interface;

public class NonlinearQuantization : IAudioCompressionAlgorithm
{
    public string Name => "Nonlinear Quantization";

    private int _quantizationLevels = 256; // افتراضي: 256 مستوى

    public void Configure(int quantizationLevels)
    {
        _quantizationLevels = quantizationLevels;
    }

    public byte[] Compress(short[] samples)
    {
        List<byte> output = new();

        // حساب الحد الأقصى للقيمة المكممة (مثلاً 255 لـ 256 مستوى)
        int maxQuantizedValue = _quantizationLevels - 1;

        foreach (var s in samples)
        {
            // تطبيع العينة إلى المدى [0, 1]
            double normalized = (s + 32768) / 65535.0;

            // تطبيق تكميم غير خطي (قانون القوة)
            double compressed = Math.Pow(normalized, 0.5);

            // تكميم إلى المستويات المحددة
            int quantized = (int)(compressed * maxQuantizedValue);
            if (quantized > maxQuantizedValue) quantized = maxQuantizedValue;
            if (quantized < 0) quantized = 0;

            output.Add((byte)quantized);
        }

        return output.ToArray();
    }

    public short[] Decompress(byte[] data)
    {
        List<short> result = new();

        int maxQuantizedValue = _quantizationLevels - 1;

        foreach (byte b in data)
        {
            // تطبيع القيمة المكممة
            double normalized = b / (double)maxQuantizedValue;

            // تطبيق القوس العكسي (رفع للقوة 2)
            double restored = Math.Pow(normalized, 2);

            // التحويل مرة أخرى إلى مدى العينة
            short sample = (short)(restored * 65535 - 32768);
            result.Add(sample);
        }

        return result.ToArray();
    }
}