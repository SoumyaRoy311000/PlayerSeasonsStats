using Microsoft.ML.OnnxRuntime;
using Serilog;

namespace LMStudioClone.Core
{
    public class DeviceManager
    {
        private readonly HardwareDetector _hardwareDetector;
        private readonly ILogger _logger = Log.ForContext<DeviceManager>();
        
        private HardwareDetector.HardwareInfo? _hardwareInfo;
        private string _currentProvider = "CPU";

        public DeviceManager(HardwareDetector hardwareDetector)
        {
            _hardwareDetector = hardwareDetector;
        }

        public async Task InitializeAsync()
        {
            _hardwareInfo = await _hardwareDetector.DetectHardwareAsync();
            _currentProvider = _hardwareInfo.RecommendedProvider;
            
            _logger.Information("Device manager initialized with provider: {Provider}", _currentProvider);
        }

        public SessionOptions CreateSessionOptions(string? preferredProvider = null)
        {
            var sessionOptions = new SessionOptions();
            
            try
            {
                var provider = preferredProvider ?? _currentProvider;
                
                switch (provider.ToUpper())
                {
                    case "CUDA":
                        if (_hardwareInfo?.HasCudaGpu == true)
                        {
                            sessionOptions.AppendExecutionProvider_CUDA();
                            _logger.Information("Using CUDA execution provider");
                        }
                        else
                        {
                            _logger.Warning("CUDA requested but not available, falling back to CPU");
                            sessionOptions.AppendExecutionProvider_CPU();
                        }
                        break;
                        
                    case "DIRECTML":
                        if (_hardwareInfo?.HasDirectMLGpu == true)
                        {
                            sessionOptions.AppendExecutionProvider_DML();
                            _logger.Information("Using DirectML execution provider");
                        }
                        else
                        {
                            _logger.Warning("DirectML requested but not available, falling back to CPU");
                            sessionOptions.AppendExecutionProvider_CPU();
                        }
                        break;
                        
                    case "OPENVINO":
                        if (_hardwareInfo?.HasOpenVINO == true)
                        {
                            // Note: OpenVINO provider configuration may need additional setup
                            _logger.Information("Using OpenVINO execution provider");
                        }
                        else
                        {
                            _logger.Warning("OpenVINO requested but not available, falling back to CPU");
                            sessionOptions.AppendExecutionProvider_CPU();
                        }
                        break;
                        
                    default:
                        sessionOptions.AppendExecutionProvider_CPU();
                        _logger.Information("Using CPU execution provider");
                        break;
                }

                // Configure session options for performance
                sessionOptions.GraphOptimizationLevel = GraphOptimizationLevel.ORT_ENABLE_ALL;
                sessionOptions.ExecutionMode = ExecutionMode.ORT_PARALLEL;
                
                // Set memory optimization
                sessionOptions.AddConfigEntry("memory.enable_memory_arena_shrinkage", "");
                
                // Set thread count based on CPU cores
                var coreCount = Environment.ProcessorCount;
                sessionOptions.IntraOpNumThreads = Math.Max(1, coreCount / 2);
                sessionOptions.InterOpNumThreads = Math.Max(1, coreCount / 4);
                
                _logger.Information("Session options configured: Threads={IntraOp}/{InterOp}, Optimization={OptLevel}", 
                    sessionOptions.IntraOpNumThreads, sessionOptions.InterOpNumThreads, sessionOptions.GraphOptimizationLevel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error configuring session options, falling back to CPU");
                sessionOptions = new SessionOptions();
                sessionOptions.AppendExecutionProvider_CPU();
            }

            return sessionOptions;
        }

        public async Task<bool> SwitchProviderAsync(string newProvider)
        {
            if (await _hardwareDetector.IsProviderAvailableAsync(newProvider))
            {
                _currentProvider = newProvider;
                _logger.Information("Switched to provider: {Provider}", newProvider);
                return true;
            }
            
            _logger.Warning("Cannot switch to provider {Provider} - not available", newProvider);
            return false;
        }

        public string GetCurrentProvider() => _currentProvider;
        
        public HardwareDetector.HardwareInfo? GetHardwareInfo() => _hardwareInfo;

        public string GetDeviceStatus()
        {
            if (_hardwareInfo == null) return "Unknown";
            
            var status = $"{_currentProvider}";
            if (_currentProvider != "CPU" && !string.IsNullOrEmpty(_hardwareInfo.GpuName))
            {
                status += $" ({_hardwareInfo.GpuName})";
            }
            else if (_currentProvider == "CPU")
            {
                status += $" ({_hardwareInfo.CpuName})";
            }
            
            return status;
        }

        public string GetMemoryStatus()
        {
            if (_hardwareInfo == null) return "Unknown";
            
            var usedMB = _hardwareInfo.TotalMemoryMB - _hardwareInfo.AvailableMemoryMB;
            var usagePercent = (double)usedMB / _hardwareInfo.TotalMemoryMB * 100;
            
            return $"{usedMB:N0} / {_hardwareInfo.TotalMemoryMB:N0} MB ({usagePercent:F1}%)";
        }

        public List<string> GetAvailableProviders()
        {
            var providers = new List<string> { "CPU" };
            
            if (_hardwareInfo?.HasCudaGpu == true) providers.Add("CUDA");
            if (_hardwareInfo?.HasDirectMLGpu == true) providers.Add("DirectML");
            if (_hardwareInfo?.HasOpenVINO == true) providers.Add("OpenVINO");
            
            return providers;
        }
    }
}

