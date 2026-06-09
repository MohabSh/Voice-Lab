using NAudio.Wave;
using System.IO;

namespace voicelab.Interface
{
    public class AudioLoader
    {
        public AudioFile Load(string path)
        {
            var info = new FileInfo(path);
            using var reader = new AudioFileReader(path);

            var fmt = reader.WaveFormat;

            // Compute real bitrate from file size and duration
            double durationSec = reader.TotalTime.TotalSeconds;
            int bitRate = durationSec > 0
                ? (int)(info.Length * 8 / durationSec)   // bps
                : fmt.AverageBytesPerSecond * 8;

            // Determine encoding from extension, not from internal WaveFormat
            string encoding = System.IO.Path.GetExtension(path).ToLowerInvariant() switch
            {
                ".mp3" => "MP3",
                ".aac" => "AAC",
                ".wma" => "WMA",
                ".wav" => fmt.Encoding.ToString(),
                _ => fmt.Encoding.ToString(),
            };

            return new AudioFile
            {
                FilePath = path,
                FileName = info.Name,
                FileSize = info.Length,
                Duration = reader.TotalTime,
                SampleRate = fmt.SampleRate,
                Channels = fmt.Channels,
                BitRate = bitRate,
                EncodingType = encoding,
            };
        }
    }
}