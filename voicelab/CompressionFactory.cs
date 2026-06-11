using voicelab.Interface;

namespace voicelab
{
    public static class CompressionFactory
    {
        // تغيير الدالة لاستقبال معامل الإعدادات
        public static IAudioCompressionAlgorithm Create(string name, int quantizationLevels)
        {
            IAudioCompressionAlgorithm algorithm = name switch
            {
                "DPCM" => new DPCM(),
                "Delta Modulation" => new DeltaModulation(),
                "Nonlinear Quantization" => new NonlinearQuantization(),
                "Predictive Differential Coding" => new PredictiveDifferentialCoding(),
                "Adaptive Delta Modulation" => new AdaptiveDeltaModulation(),
                _ => throw new System.Exception($"Unknown algorithm: {name}")
            };

            // تطبيق الإعدادات على الخوارزمية
            algorithm.Configure(quantizationLevels);

            return algorithm;
        }
    }
}