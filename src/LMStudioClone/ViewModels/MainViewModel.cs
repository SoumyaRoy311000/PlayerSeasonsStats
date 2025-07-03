using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Windows.Input;
using LMStudioClone.Core;
using LMStudioClone.Models;
using Serilog;

namespace LMStudioClone.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly DeviceManager _deviceManager;
        private readonly HuggingFaceClient _huggingFaceClient;
        private readonly ILogger _logger = Log.ForContext<MainViewModel>();

        [ObservableProperty]
        private object? currentView;

        [ObservableProperty]
        private string statusMessage = "Ready";

        [ObservableProperty]
        private bool isProcessing = false;

        [ObservableProperty]
        private double progressValue = 0;

        [ObservableProperty]
        private string inferenceStats = "";

        [ObservableProperty]
        private ObservableCollection<ModelInfo> availableModels = new();

        [ObservableProperty]
        private ModelInfo? selectedModel;

        [ObservableProperty]
        private HardwareStatus hardwareStatus = new();

        public MainViewModel(DeviceManager deviceManager, HuggingFaceClient huggingFaceClient)
        {
            _deviceManager = deviceManager;
            _huggingFaceClient = huggingFaceClient;
            
            InitializeAsync();
        }

        private async void InitializeAsync()
        {
            try
            {
                StatusMessage = "Initializing hardware...";
                await _deviceManager.InitializeAsync();
                
                UpdateHardwareStatus();
                
                StatusMessage = "Loading available models...";
                await LoadAvailableModelsAsync();
                
                // Set default view
                NavigateToView("Chat");
                
                StatusMessage = "Ready";
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error during initialization");
                StatusMessage = $"Initialization error: {ex.Message}";
            }
        }

        private void UpdateHardwareStatus()
        {
            var info = _deviceManager.GetHardwareInfo();
            if (info != null)
            {
                HardwareStatus = new HardwareStatus
                {
                    DeviceName = _deviceManager.GetDeviceStatus(),
                    MemoryUsage = _deviceManager.GetMemoryStatus(),
                    StatusColor = _deviceManager.GetCurrentProvider() == "CPU" ? "#FFA500" : "#00FF00"
                };
            }
        }

        private async Task LoadAvailableModelsAsync()
        {
            try
            {
                // Add some default models
                AvailableModels.Clear();
                
                // Add placeholder models - in real implementation, these would come from model cache
                AvailableModels.Add(new ModelInfo
                {
                    Name = "No Model Loaded",
                    Status = "Select a model to begin",
                    Path = "",
                    IsLoaded = false
                });

                SelectedModel = AvailableModels.First();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error loading available models");
            }
        }

        public void NavigateToView(string viewName)
        {
            try
            {
                CurrentView = viewName switch
                {
                    "Chat" => new Views.ChatView(),
                    "Documents" => new Views.DocumentView(),
                    "Images" => new Views.ImageView(),
                    "Generation" => new Views.GenerationView(),
                    "Workflows" => new Views.WorkflowView(),
                    _ => new Views.ChatView()
                };
                
                _logger.Information("Navigated to view: {ViewName}", viewName);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error navigating to view: {ViewName}", viewName);
            }
        }

        [RelayCommand]
        private void NewConversation()
        {
            NavigateToView("Chat");
            StatusMessage = "New conversation started";
        }

        [RelayCommand]
        private async Task OpenDocument()
        {
            // TODO: Implement document opening
            NavigateToView("Documents");
            StatusMessage = "Document workspace opened";
        }

        [RelayCommand]
        private async Task ImportImage()
        {
            // TODO: Implement image import
            NavigateToView("Images");
            StatusMessage = "Image workspace opened";
        }

        [RelayCommand]
        private async Task OpenSettings()
        {
            // TODO: Implement settings dialog
            StatusMessage = "Settings opened";
        }

        [RelayCommand]
        private void Exit()
        {
            System.Windows.Application.Current.Shutdown();
        }

        [RelayCommand]
        private async Task BrowseModels()
        {
            try
            {
                IsProcessing = true;
                StatusMessage = "Browsing Hugging Face models...";
                
                // TODO: Implement model browsing
                await Task.Delay(1000); // Placeholder
                
                StatusMessage = "Model browser opened";
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error browsing models");
                StatusMessage = $"Error: {ex.Message}";
            }
            finally
            {
                IsProcessing = false;
            }
        }

        [RelayCommand]
        private async Task DownloadModel()
        {
            try
            {
                IsProcessing = true;
                StatusMessage = "Downloading model...";
                
                // TODO: Implement model download
                await Task.Delay(2000); // Placeholder
                
                StatusMessage = "Model downloaded successfully";
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error downloading model");
                StatusMessage = $"Error: {ex.Message}";
            }
            finally
            {
                IsProcessing = false;
            }
        }

        [RelayCommand]
        private async Task ManageModels()
        {
            // TODO: Implement model management
            StatusMessage = "Model manager opened";
        }

        [RelayCommand]
        private void ShowHardwareInfo()
        {
            var info = _deviceManager.GetHardwareInfo();
            if (info != null)
            {
                var message = $"CPU: {info.CpuName}\n" +
                             $"GPU: {info.GpuName}\n" +
                             $"Memory: {info.TotalMemoryMB:N0} MB\n" +
                             $"Provider: {_deviceManager.GetCurrentProvider()}\n" +
                             $"Available Providers: {string.Join(", ", _deviceManager.GetAvailableProviders())}";
                
                System.Windows.MessageBox.Show(message, "Hardware Information", 
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
            }
        }

        [RelayCommand]
        private void ShowPerformance()
        {
            // TODO: Implement performance monitor
            StatusMessage = "Performance monitor opened";
        }

        [RelayCommand]
        private void About()
        {
            var message = "LM Studio Clone - Enhanced AI Workstation\n\n" +
                         "Features:\n" +
                         "• Local LLM inference with GPU acceleration\n" +
                         "• Document processing and editing\n" +
                         "• Image analysis and editing\n" +
                         "• AI image generation\n" +
                         "• Multi-modal workflows\n\n" +
                         "Built with C# .NET and ONNX Runtime";
            
            System.Windows.MessageBox.Show(message, "About LM Studio Clone", 
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
        }
    }

    public class ModelInfo
    {
        public string Name { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public bool IsLoaded { get; set; }
        public long SizeBytes { get; set; }
        public string ModelType { get; set; } = string.Empty;
    }

    public class HardwareStatus
    {
        public string DeviceName { get; set; } = string.Empty;
        public string MemoryUsage { get; set; } = string.Empty;
        public string StatusColor { get; set; } = "#808080";
    }
}

