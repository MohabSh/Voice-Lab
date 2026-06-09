using voicelab.Interface;

namespace voicelab
{
    public static class CompressionFactory
    {
        public static IAudioCompressionAlgorithm Create(string name)
        {
            return name switch
            {
                "DPCM" => new DPCM(),
                "Delta Modulation" => new DeltaModulation(),
                "Nonlinear Quantization" => new NonlinearQuantization(),
                "Predictive Differential Coding" => new PredictiveDifferentialCoding(),
                "Adaptive Delta Modulation" => new AdaptiveDeltaModulation(),
                _ => throw new System.Exception($"Unknown algorithm: {name}")
            };
        }
    }
}