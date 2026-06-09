using System;
using System.Collections.Generic;
using System.Text;

namespace voicelab.Interface
{
    public class AudioFile
    {
        public string FilePath { get; set; }
        public string FileName { get; set; }
        public long FileSize { get; set; }
        public TimeSpan Duration { get; set; }
        public int SampleRate { get; set; }
        public int Channels { get; set; }
        public int BitRate { get; set; }
        public string EncodingType { get; set; }
    }
}