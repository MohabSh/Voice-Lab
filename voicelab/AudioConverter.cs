using NAudio.Wave;
using System;

namespace AudioCompressor.Services
{
    public static class AudioConverter
    {
        /// <summary>
        /// يحوّل أي ملف صوتي (MP3/WAV/AAC...) إلى مصفوفة PCM 16-bit
        /// </summary>
        public static short[] ToPCM(string filePath)
        {
            using var reader = new AudioFileReader(filePath);

            // نحوّل كل شيء إلى PCM 16-bit Mono 44100 Hz
            var resampler = new MediaFoundationResampler(reader,
                new WaveFormat(44100, 16, 1));

            resampler.ResamplerQuality = 60;

            var samples = new System.Collections.Generic.List<short>();
            byte[] buffer = new byte[4096];
            int bytesRead;

            while ((bytesRead = resampler.Read(buffer, 0, buffer.Length)) > 0)
            {
                for (int i = 0; i < bytesRead - 1; i += 2)
                {
                    short sample = BitConverter.ToInt16(buffer, i);
                    samples.Add(sample);
                }
            }

            return samples.ToArray();
        }

        /// <summary>
        /// يحفظ مصفوفة PCM كملف WAV
        /// </summary>
        public static void SaveAsWav(short[] samples, string outputPath,
            int sampleRate = 44100, int channels = 1)
        {
            var format = new WaveFormat(sampleRate, 16, channels);

            using var writer = new WaveFileWriter(outputPath, format);
            foreach (var s in samples)
            {
                writer.WriteSample(s / 32768f);
            }
        }
    }
}