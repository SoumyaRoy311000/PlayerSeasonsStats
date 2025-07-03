using LMStudioClone.Core;
using Serilog;

namespace LMStudioClone.Inference
{
    public class LLMEngine
    {
        private readonly InferenceProvider _inferenceProvider;
        private readonly TokenizerService _tokenizerService;
        private readonly ILogger _logger = Log.ForContext<LLMEngine>();

        public LLMEngine(InferenceProvider inferenceProvider, TokenizerService tokenizerService)
        {
            _inferenceProvider = inferenceProvider;
            _tokenizerService = tokenizerService;
        }

        public async Task<string> GenerateResponseAsync(string prompt, GenerationConfig config)
        {
            try
            {
                if (!_inferenceProvider.IsModelLoaded)
                {
                    _logger.Warning("No model loaded for text generation");
                    return "No model loaded. Please load a model first.";
                }

                _logger.Information("Generating response for prompt: {Prompt}", prompt.Substring(0, Math.Min(50, prompt.Length)));

                // Tokenize input
                var tokens = await _tokenizerService.TokenizeAsync(prompt);
                if (tokens == null || tokens.Length == 0)
                {
                    return "Error: Could not tokenize input";
                }

                // Prepare inputs for inference
                var inputs = new Dictionary<string, object>
                {
                    ["input_ids"] = tokens
                };

                // Run inference
                var output = await _inferenceProvider.RunInferenceAsync(inputs);
                if (output == null)
                {
                    return "Error: Inference failed";
                }

                // Detokenize output
                var response = await _tokenizerService.DetokenizeAsync(output);
                
                _logger.Information("Generated response: {Response}", response?.Substring(0, Math.Min(50, response?.Length ?? 0)));
                return response ?? "Error: Could not decode response";
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error during text generation");
                return $"Error: {ex.Message}";
            }
        }

        public async Task<IAsyncEnumerable<string>> GenerateStreamingResponseAsync(string prompt, GenerationConfig config)
        {
            // TODO: Implement streaming generation
            // For now, return the full response as a single item
            var response = await GenerateResponseAsync(prompt, config);
            return AsyncEnumerable.Repeat(response, 1);
        }

        public bool IsModelLoaded => _inferenceProvider.IsModelLoaded;
        
        public string? CurrentModelPath => _inferenceProvider.CurrentModelPath;
    }
}

