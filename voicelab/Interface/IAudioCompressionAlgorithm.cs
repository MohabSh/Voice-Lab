using System;
using System.Collections.Generic;
using System.Text;

namespace voicelab.Interface
{
    public interface IAudioCompressionAlgorithm
    {
        string Name { get; }
        void Configure(int quantizationLevels);
        byte[] Compress(short[] samples);

        short[] Decompress(byte[] data);
    }
}
