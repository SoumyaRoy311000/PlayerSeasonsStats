namespace LMStudioClone.Inference
{
    public class GenerationConfig
    {
        public float Temperature { get; set; } = 0.7f;
        public float TopP { get; set; } = 0.9f;
        public int TopK { get; set; } = 50;
        public int MaxTokens { get; set; } = 512;
        public int MinTokens { get; set; } = 1;
        public float RepetitionPenalty { get; set; } = 1.1f;
        public int Seed { get; set; } = -1; // -1 for random seed
        public string[] StopSequences { get; set; } = Array.Empty<string>();
        public bool DoSample { get; set; } = true;
        public int NumBeams { get; set; } = 1;
        public float LengthPenalty { get; set; } = 1.0f;
        public bool EarlyStopping { get; set; } = false;
        
        public static GenerationConfig Default => new GenerationConfig();
        
        public static GenerationConfig Creative => new GenerationConfig
        {
            Temperature = 1.0f,
            TopP = 0.95f,
            TopK = 100,
            RepetitionPenalty = 1.05f
        };
        
        public static GenerationConfig Precise => new GenerationConfig
        {
            Temperature = 0.3f,
            TopP = 0.8f,
            TopK = 20,
            RepetitionPenalty = 1.2f
        };
        
        public static GenerationConfig Balanced => new GenerationConfig
        {
            Temperature = 0.7f,
            TopP = 0.9f,
            TopK = 50,
            RepetitionPenalty = 1.1f
        };
        
        public GenerationConfig Clone()
        {
            return new GenerationConfig
            {
                Temperature = Temperature,
                TopP = TopP,
                TopK = TopK,
                MaxTokens = MaxTokens,
                MinTokens = MinTokens,
                RepetitionPenalty = RepetitionPenalty,
                Seed = Seed,
                StopSequences = (string[])StopSequences.Clone(),
                DoSample = DoSample,
                NumBeams = NumBeams,
                LengthPenalty = LengthPenalty,
                EarlyStopping = EarlyStopping
            };
        }
        
        public void Validate()
        {
            Temperature = Math.Max(0.01f, Math.Min(2.0f, Temperature));
            TopP = Math.Max(0.01f, Math.Min(1.0f, TopP));
            TopK = Math.Max(1, Math.Min(1000, TopK));
            MaxTokens = Math.Max(1, Math.Min(8192, MaxTokens));
            MinTokens = Math.Max(0, Math.Min(MaxTokens, MinTokens));
            RepetitionPenalty = Math.Max(0.1f, Math.Min(2.0f, RepetitionPenalty));
            NumBeams = Math.Max(1, Math.Min(10, NumBeams));
            LengthPenalty = Math.Max(0.1f, Math.Min(2.0f, LengthPenalty));
        }
        
        public override string ToString()
        {
            return $"GenerationConfig(T={Temperature:F2}, TopP={TopP:F2}, TopK={TopK}, MaxTokens={MaxTokens})";
        }
    }
}

