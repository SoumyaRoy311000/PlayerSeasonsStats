using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Serilog;
using System.Windows;
using LMStudioClone.Core;
using LMStudioClone.Services;
using LMStudioClone.ViewModels;

namespace LMStudioClone
{
    public partial class App : Application
    {
        private IHost? _host;

        protected override async void OnStartup(StartupEventArgs e)
        {
            // Configure Serilog
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File("logs/app-.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            try
            {
                // Build host with dependency injection
                _host = Host.CreateDefaultBuilder()
                    .UseSerilog()
                    .ConfigureServices((context, services) =>
                    {
                        ConfigureServices(services);
                    })
                    .Build();

                await _host.StartAsync();

                // Get main window from DI container
                var mainWindow = _host.Services.GetRequiredService<MainWindow>();
                mainWindow.Show();

                base.OnStartup(e);
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application failed to start");
                MessageBox.Show($"Application failed to start: {ex.Message}", "Fatal Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown();
            }
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // Core Services
            services.AddSingleton<HardwareDetector>();
            services.AddSingleton<DeviceManager>();
            services.AddSingleton<InferenceProvider>();

            // Model Services
            services.AddSingleton<HuggingFaceClient>();
            services.AddSingleton<ModelDownloader>();
            services.AddSingleton<ModelCache>();

            // Inference Services
            services.AddSingleton<LLMEngine>();
            services.AddSingleton<TokenizerService>();

            // Application Services
            services.AddSingleton<ConversationManager>();

            // ViewModels
            services.AddTransient<MainViewModel>();
            services.AddTransient<ChatViewModel>();

            // Views
            services.AddTransient<MainWindow>();
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            if (_host != null)
            {
                await _host.StopAsync();
                _host.Dispose();
            }

            Log.CloseAndFlush();
            base.OnExit(e);
        }
    }
}

