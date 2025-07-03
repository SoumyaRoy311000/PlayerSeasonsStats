using Microsoft.ML.OnnxRuntime;
using System.Management;
using Serilog;

namespace LMStudioClone.Core
{
    public class HardwareDetector
    {
        private readonly ILogger _logger = Log.ForContext<HardwareDetector>();

        public class HardwareInfo
        {
            public bool HasCudaGpu { get; set; }
            public bool HasDirectMLGpu { get; set; }
            public bool HasOpenVINO { get; set; }
            public string CpuName { get; set; } = string.Empty;
            public string GpuName { get; set; } = string.Empty;
            public long TotalMemoryMB { get; set; }
            public long AvailableMemoryMB { get; set; }
            public string RecommendedProvider { get; set; } = "CPU";
        }

        public async Task<HardwareInfo> DetectHardwareAsync()
        {
            var info = new HardwareInfo();

            try
            {
                // Detect CPU
                await DetectCpuAsync(info);
                
                // Detect Memory
                await DetectMemoryAsync(info);
                
                // Detect GPU capabilities
                await DetectGpuCapabilitiesAsync(info);
                
                // Determine recommended provider
                DetermineRecommendedProvider(info);

                _logger.Information("Hardware detection completed: CPU={CpuName}, GPU={GpuName}, Provider={Provider}", 
                    info.CpuName, info.GpuName, info.RecommendedProvider);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error during hardware detection");
            }

            return info;
        }

        private async Task DetectCpuAsync(HardwareInfo info)
        {
            await Task.Run(() =>
            {
                try
                {
                    using var searcher = new ManagementObjectSearcher("SELECT Name FROM Win32_Processor");
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        info.CpuName = obj["Name"]?.ToString() ?? "Unknown CPU";
                        break;
                    }
                }
                catch (Exception ex)
                {
                    _logger.Warning(ex, "Could not detect CPU information");
                    info.CpuName = "Unknown CPU";
                }
            });
        }

        private async Task DetectMemoryAsync(HardwareInfo info)
        {
            await Task.Run(() =>
            {
                try
                {
                    using var searcher = new ManagementObjectSearcher("SELECT TotalPhysicalMemory FROM Win32_ComputerSystem");
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        if (obj["TotalPhysicalMemory"] != null)
                        {
                            info.TotalMemoryMB = Convert.ToInt64(obj["TotalPhysicalMemory"]) / (1024 * 1024);
                        }
                        break;
                    }

                    // Get available memory
                    using var perfCounter = new ManagementObjectSearcher("SELECT AvailableBytes FROM Win32_PerfRawData_PerfOS_Memory");
                    foreach (ManagementObject obj in perfCounter.Get())
                    {
                        if (obj["AvailableBytes"] != null)
                        {
                            info.AvailableMemoryMB = Convert.ToInt64(obj["AvailableBytes"]) / (1024 * 1024);
                        }
                        break;
                    }
                }
                catch (Exception ex)
                {
                    _logger.Warning(ex, "Could not detect memory information");
                }
            });
        }

        private async Task DetectGpuCapabilitiesAsync(HardwareInfo info)
        {
            await Task.Run(() =>
            {
                try
                {
                    // Detect GPU hardware
                    using var searcher = new ManagementObjectSearcher("SELECT Name FROM Win32_VideoController");
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        var gpuName = obj["Name"]?.ToString() ?? "";
                        if (!string.IsNullOrEmpty(gpuName) && !gpuName.Contains("Microsoft Basic"))
                        {
                            info.GpuName = gpuName;
                            break;
                        }
                    }

                    // Test ONNX Runtime providers
                    TestOnnxProviders(info);
                }
                catch (Exception ex)
                {
                    _logger.Warning(ex, "Could not detect GPU capabilities");
                }
            });
        }

        private void TestOnnxProviders(HardwareInfo info)
        {
            try
            {
                // Test CUDA provider
                var availableProviders = OrtEnv.Instance().GetAvailableProviders();
                
                info.HasCudaGpu = availableProviders.Contains("CUDAExecutionProvider");
                info.HasDirectMLGpu = availableProviders.Contains("DmlExecutionProvider");
                info.HasOpenVINO = availableProviders.Contains("OpenVINOExecutionProvider");

                _logger.Information("Available ONNX providers: {Providers}", string.Join(", ", availableProviders));
            }
            catch (Exception ex)
            {
                _logger.Warning(ex, "Could not test ONNX Runtime providers");
            }
        }

        private void DetermineRecommendedProvider(HardwareInfo info)
        {
            // Priority: CUDA > DirectML > OpenVINO > CPU
            if (info.HasCudaGpu && info.GpuName.Contains("NVIDIA", StringComparison.OrdinalIgnoreCase))
            {
                info.RecommendedProvider = "CUDA";
            }
            else if (info.HasDirectMLGpu)
            {
                info.RecommendedProvider = "DirectML";
            }
            else if (info.HasOpenVINO)
            {
                info.RecommendedProvider = "OpenVINO";
            }
            else
            {
                info.RecommendedProvider = "CPU";
            }
        }

        public async Task<bool> IsProviderAvailableAsync(string providerName)
        {
            return await Task.Run(() =>
            {
                try
                {
                    var availableProviders = OrtEnv.Instance().GetAvailableProviders();
                    return providerName.ToLower() switch
                    {
                        "cuda" => availableProviders.Contains("CUDAExecutionProvider"),
                        "directml" => availableProviders.Contains("DmlExecutionProvider"),
                        "openvino" => availableProviders.Contains("OpenVINOExecutionProvider"),
                        "cpu" => true,
                        _ => false
                    };
                }
                catch
                {
                    return false;
                }
            });
        }
    }
}

