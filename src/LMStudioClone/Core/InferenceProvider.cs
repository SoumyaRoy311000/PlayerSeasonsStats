using Microsoft.ML.OnnxRuntime;
using Serilog;

namespace LMStudioClone.Core
{
    public class InferenceProvider : IDisposable
    {
        private readonly DeviceManager _deviceManager;
        private readonly ILogger _logger = Log.ForContext<InferenceProvider>();
        
        private InferenceSession? _currentSession;
        private string? _currentModelPath;
        private bool _disposed = false;

        public InferenceProvider(DeviceManager deviceManager)
        {
            _deviceManager = deviceManager;
        }

        public async Task<bool> LoadModelAsync(string modelPath, string? preferredProvider = null)
        {
            try
            {
                // Dispose existing session
                DisposeCurrentSession();

                if (!File.Exists(modelPath))
                {
                    _logger.Error("Model file not found: {ModelPath}", modelPath);
                    return false;
                }

                _logger.Information("Loading model: {ModelPath}", modelPath);

                // Create session options with appropriate provider
                var sessionOptions = _deviceManager.CreateSessionOptions(preferredProvider);
                
                // Load the model
                _currentSession = new InferenceSession(modelPath, sessionOptions);
                _currentModelPath = modelPath;

                // Log model information
                LogModelInfo();

                _logger.Information("Model loaded successfully: {ModelPath}", modelPath);
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to load model: {ModelPath}", modelPath);
                DisposeCurrentSession();
                return false;
            }
        }

        public async Task<float[]?> RunInferenceAsync(Dictionary<string, object> inputs)
        {
            if (_currentSession == null)
            {
                _logger.Warning("No model loaded for inference");
                return null;
            }

            try
            {
                // Convert inputs to NamedOnnxValue
                var onnxInputs = new List<NamedOnnxValue>();
                
                foreach (var input in inputs)
                {
                    if (input.Value is float[] floatArray)
                    {
                        var tensor = new DenseTensor<float>(floatArray, new[] { 1, floatArray.Length });
                        onnxInputs.Add(NamedOnnxValue.CreateFromTensor(input.Key, tensor));
                    }
                    else if (input.Value is long[] longArray)
                    {
                        var tensor = new DenseTensor<long>(longArray, new[] { 1, longArray.Length });
                        onnxInputs.Add(NamedOnnxValue.CreateFromTensor(input.Key, tensor));
                    }
                    else if (input.Value is int[] intArray)
                    {
                        var longArray2 = intArray.Select(x => (long)x).ToArray();
                        var tensor = new DenseTensor<long>(longArray2, new[] { 1, longArray2.Length });
                        onnxInputs.Add(NamedOnnxValue.CreateFromTensor(input.Key, tensor));
                    }
                }

                // Run inference
                using var results = await Task.Run(() => _currentSession.Run(onnxInputs));
                
                // Extract output (assuming single float array output)
                var output = results.FirstOrDefault();
                if (output?.AsTensor<float>() is var tensor && tensor != null)
                {
                    return tensor.ToArray();
                }

                _logger.Warning("No valid output from inference");
                return null;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Inference failed");
                return null;
            }
        }

        public bool IsModelLoaded => _currentSession != null;
        
        public string? CurrentModelPath => _currentModelPath;

        public ModelMetadata? GetModelMetadata()
        {
            if (_currentSession == null) return null;

            try
            {
                var metadata = _currentSession.ModelMetadata;
                return new ModelMetadata
                {
                    ProducerName = metadata.ProducerName,
                    GraphName = metadata.GraphName,
                    Domain = metadata.Domain,
                    Description = metadata.Description,
                    GraphDescription = metadata.GraphDescription,
                    Version = metadata.Version
                };
            }
            catch (Exception ex)
            {
                _logger.Warning(ex, "Could not retrieve model metadata");
                return null;
            }
        }

        public List<NodeMetadata> GetInputMetadata()
        {
            if (_currentSession == null) return new List<NodeMetadata>();

            try
            {
                return _currentSession.InputMetadata.Select(input => new NodeMetadata
                {
                    Name = input.Key,
                    ElementType = input.Value.ElementType.ToString(),
                    Dimensions = input.Value.Dimensions.ToArray()
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.Warning(ex, "Could not retrieve input metadata");
                return new List<NodeMetadata>();
            }
        }

        public List<NodeMetadata> GetOutputMetadata()
        {
            if (_currentSession == null) return new List<NodeMetadata>();

            try
            {
                return _currentSession.OutputMetadata.Select(output => new NodeMetadata
                {
                    Name = output.Key,
                    ElementType = output.Value.ElementType.ToString(),
                    Dimensions = output.Value.Dimensions.ToArray()
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.Warning(ex, "Could not retrieve output metadata");
                return new List<NodeMetadata>();
            }
        }

        private void LogModelInfo()
        {
            if (_currentSession == null) return;

            try
            {
                var metadata = GetModelMetadata();
                var inputs = GetInputMetadata();
                var outputs = GetOutputMetadata();

                _logger.Information("Model Info - Producer: {Producer}, Version: {Version}", 
                    metadata?.ProducerName, metadata?.Version);
                
                _logger.Information("Inputs: {Inputs}", 
                    string.Join(", ", inputs.Select(i => $"{i.Name}[{string.Join(",", i.Dimensions)}]")));
                
                _logger.Information("Outputs: {Outputs}", 
                    string.Join(", ", outputs.Select(o => $"{o.Name}[{string.Join(",", o.Dimensions)}]")));
            }
            catch (Exception ex)
            {
                _logger.Warning(ex, "Could not log model information");
            }
        }

        private void DisposeCurrentSession()
        {
            _currentSession?.Dispose();
            _currentSession = null;
            _currentModelPath = null;
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                DisposeCurrentSession();
                _disposed = true;
            }
        }
    }

    public class ModelMetadata
    {
        public string ProducerName { get; set; } = string.Empty;
        public string GraphName { get; set; } = string.Empty;
        public string Domain { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string GraphDescription { get; set; } = string.Empty;
        public long Version { get; set; }
    }

    public class NodeMetadata
    {
        public string Name { get; set; } = string.Empty;
        public string ElementType { get; set; } = string.Empty;
        public int[] Dimensions { get; set; } = Array.Empty<int>();
    }
}

