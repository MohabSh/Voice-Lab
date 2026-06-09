namespace voicelab
{
    /// <summary>
    /// Holds all metrics generated after a compression run.
    /// </summary>
    public class CompressionReport
    {
        public string Algorithm { get; set; }
        public int SampleRate { get; set; }   // Hz
        public int QuantizationLevels { get; set; }
        public int SampleCount { get; set; }
        public int Channels { get; set; }
        public int OriginalBitRate { get; set; }   // bps
        public string EncodingType { get; set; }

        public double OriginalSizeKB { get; set; }
        public double CompressedSizeKB { get; set; }

        /// <summary>OriginalSize / CompressedSize</summary>
        public double Ratio { get; set; }

        /// <summary>Percentage of space saved (0–100).</summary>
        public double SavingPercent { get; set; }

        public long ElapsedMs { get; set; }

        public override string ToString() =>
            $"{Algorithm} | {Ratio:F2}x | {SavingPercent:F1}% saved | {ElapsedMs} ms";
    }
}
