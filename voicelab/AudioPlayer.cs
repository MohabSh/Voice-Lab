using NAudio.Wave;
using System;

namespace AudioCompressor.Services
{
    public class AudioPlayer
    {
        private WaveOutEvent outputDevice;
        private IWaveProvider currentProvider;

        // ── Play a normal audio file (mp3/wav/aac…) ───────────────
        public void Play(string path)
        {
            Stop();
            var audioFile = new AudioFileReader(path);
            outputDevice = new WaveOutEvent();
            currentProvider = audioFile;
            outputDevice.Init(audioFile);
            outputDevice.Play();
        }

        // ── Play raw PCM short[] directly from memory ─────────────
        public void PlayFromSamples(short[] samples, int sampleRate = 44100, int channels = 1)
        {
            Stop();

            // Convert short[] → byte[]
            var bytes = new byte[samples.Length * 2];
            Buffer.BlockCopy(samples, 0, bytes, 0, bytes.Length);

            var format = new WaveFormat(sampleRate, 16, channels);
            var provider = new RawSourceWaveStream(
                               new System.IO.MemoryStream(bytes), format);

            outputDevice = new WaveOutEvent();
            currentProvider = provider;
            outputDevice.Init(provider);
            outputDevice.Play();
        }

        public void Stop()
        {
            outputDevice?.Stop();
            outputDevice?.Dispose();
            outputDevice = null;

            if (currentProvider is IDisposable d)
                d.Dispose();
            currentProvider = null;
        }
    }
}