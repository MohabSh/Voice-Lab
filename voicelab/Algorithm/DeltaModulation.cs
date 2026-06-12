using System;
using System.Collections.Generic;
using voicelab.Interface;

public class DeltaModulation : IAudioCompressionAlgorithm
{
    public string Name => "Delta Modulation";
    
    private int _quantizationLevels = 2; // افتراضي: 2 مستويات
    
    public void Configure(int quantizationLevels)
    {
        _quantizationLevels = quantizationLevels;
    }

    public byte[] Compress(short[] samples)
    {
        List<byte> result = new();
        short prev = 0;
        byte currentByte = 0;
        int bitIndex = 0;
        
        int bitsPerSample = (int)System.Math.Log2(_quantizationLevels);
        int samplesPerByte = 8 / bitsPerSample;
        int stepSize = 65536 / _quantizationLevels;
        int maxValue = 32767;

        foreach (var s in samples)
        {
            int diff = s - prev;
            int quantizedIndex = (diff + maxValue) / stepSize;
            if (quantizedIndex >= _quantizationLevels) quantizedIndex = _quantizationLevels - 1;
            if (quantizedIndex < 0) quantizedIndex = 0;
            
            currentByte |= (byte)(quantizedIndex << (8 - bitsPerSample - (bitIndex * bitsPerSample)));
            bitIndex++;
            
            if (bitIndex == samplesPerByte)
            {
                result.Add(currentByte);
                currentByte = 0;
                bitIndex = 0;
            }
            
            // إعادة بناء القيمة
            int dequantizedValue = (quantizedIndex * stepSize) - maxValue;
            prev = (short)(prev + dequantizedValue);
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
        
        int bitsPerSample = (int)System.Math.Log2(_quantizationLevels);
        int samplesPerByte = 8 / bitsPerSample;
        int stepSize = 65536 / _quantizationLevels;
        int maxValue = 32767;

        foreach (var b in data)
        {
            for (int i = 0; i < samplesPerByte; i++)
            {
                int shift = 8 - bitsPerSample - (i * bitsPerSample);
                int quantizedIndex = (b >> shift) & ((1 << bitsPerSample) - 1);
                
                int dequantizedValue = (quantizedIndex * stepSize) - maxValue;
                value = (short)(value + dequantizedValue);
                result.Add(value);
            }
        }

        return result.ToArray();
    }
}