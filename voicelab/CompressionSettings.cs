using System;
using System.Collections.Generic;
using System.Text;

namespace voicelab
{
    public class CompressionSettings
    {
        public int SampleRate { get; set; }

        public int QuantizationLevels { get; set; }

        public string AlgorithmName { get; set; }
    }
}
