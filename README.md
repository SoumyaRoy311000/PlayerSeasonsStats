# LM Studio Clone - Enhanced AI Workstation

An advanced C# .NET desktop application that extends the capabilities of LM Studio with document processing, image editing, and AI image generation features.

## 🚀 Features

### Core LLM Capabilities
- 🤖 **Local LLM Inference** - Run language models locally with GPU acceleration
- 💬 **Interactive Chat Interface** - Modern chat UI with conversation management
- ⚙️ **Advanced Parameters** - Fine-tune generation with temperature, top-p, max tokens
- 🔄 **Model Management** - Download and manage models from Hugging Face Hub

### Enhanced Capabilities
- 📄 **Document Processing** - Import, edit, and analyze PDF, DOCX, and Markdown files
- 🖼️ **Image Analysis** - Upload and analyze images with vision models
- 🎨 **AI Image Generation** - Generate images using Stable Diffusion models
- 🔗 **Multi-Modal Workflows** - Combine text, image, and generation capabilities

### Hardware Optimization
- 🚀 **GPU Acceleration** - Automatic detection and use of NVIDIA CUDA, AMD DirectML, Intel OpenVINO
- 💾 **Smart Memory Management** - Efficient model loading and resource utilization
- 🔧 **Hardware Detection** - Automatic optimization based on available hardware

## 🛠️ Technology Stack

- **Framework**: .NET 8.0 with WPF
- **AI/ML**: ONNX Runtime with multi-provider support
- **UI**: ModernWPF for contemporary Windows design
- **Architecture**: MVVM with dependency injection
- **Models**: Hugging Face Hub integration

## 📋 Prerequisites

- Windows 10/11 (x64)
- .NET 8.0 Runtime
- Visual Studio 2022 (for development)
- Optional: NVIDIA GPU with CUDA 11.8+ for GPU acceleration

## 🚀 Quick Start

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/yourusername/LMStudioClone.git
   cd LMStudioClone
   ```

2. **Build the solution**
   ```bash
   dotnet build
   ```

3. **Run the application**
   ```bash
   dotnet run --project src/LMStudioClone
   ```

### First Run

1. **Hardware Detection** - The app will automatically detect your hardware capabilities
2. **Model Download** - Browse and download models from Hugging Face
3. **Start Chatting** - Begin conversations with your local AI models

## 📖 Usage Guide

### Chat Workspace
- Select a model from the sidebar
- Adjust generation parameters (temperature, max tokens, top-p)
- Type your message and press Send
- View conversation history and export chats

### Document Workspace
- Import PDF, DOCX, or Markdown files
- Use AI to summarize, analyze, or edit documents
- Export processed documents in various formats

### Image Workspace
- Upload images for analysis
- Generate descriptions and answer questions about images
- Basic image editing capabilities

### Generation Workspace
- Create AI-generated images from text prompts
- Adjust generation parameters (steps, guidance scale, seed)
- Save and manage generated images

### Multi-Modal Workflows
- Combine text and image processing
- Generate images based on document content
- Create comprehensive AI-assisted content

## 🔧 Configuration

### Model Settings
- **Cache Directory**: `%LocalAppData%/LMStudioClone/Models`
- **Supported Formats**: ONNX, SafeTensors, PyTorch
- **Auto-Conversion**: Automatic PyTorch to ONNX conversion

### Hardware Settings
- **Provider Priority**: CUDA > DirectML > OpenVINO > CPU
- **Memory Management**: Automatic optimization based on available RAM
- **GPU Detection**: Automatic detection of compatible GPUs

### Generation Parameters
- **Temperature**: 0.1-2.0 (creativity vs consistency)
- **Top-P**: 0.1-1.0 (nucleus sampling)
- **Max Tokens**: 1-8192 (response length)
- **Repetition Penalty**: 0.1-2.0 (avoid repetition)

## 🏗️ Architecture

The application follows a layered architecture:

```
┌─────────────────────────────────────────┐
│           Presentation Layer            │
│  (WPF Views, ViewModels, Converters)   │
├─────────────────────────────────────────┤
│           Application Layer             │
│    (Services, Workflows, Managers)     │
├─────────────────────────────────────────┤
│             Domain Layer                │
│   (LLM Engine, Vision, Generation)     │
├─────────────────────────────────────────┤
│          Infrastructure Layer           │
│  (ONNX Runtime, Hardware, Storage)     │
└─────────────────────────────────────────┘
```

See [Architecture Documentation](docs/Architecture.md) for detailed information.

## 🧪 Development

### Building from Source

1. **Prerequisites**
   - Visual Studio 2022 with .NET 8.0 SDK
   - Git for version control

2. **Clone and Build**
   ```bash
   git clone https://github.com/yourusername/LMStudioClone.git
   cd LMStudioClone
   dotnet restore
   dotnet build
   ```

3. **Run Tests**
   ```bash
   dotnet test
   ```

### Project Structure
```
LMStudioClone/
├── src/LMStudioClone/          # Main application
│   ├── Core/                   # Hardware and inference
│   ├── Models/                 # Model management
│   ├── Views/                  # WPF user controls
│   ├── ViewModels/             # MVVM view models
│   ├── Services/               # Application services
│   └── Inference/              # AI inference engines
├── tests/LMStudioClone.Tests/  # Unit tests
└── docs/                       # Documentation
```

### Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 📊 Performance

### Benchmarks
- **Model Loading**: 2-10 seconds (depending on model size)
- **Text Generation**: 10-50 tokens/second (hardware dependent)
- **Memory Usage**: 2-16 GB (model dependent)
- **GPU Acceleration**: 3-10x faster than CPU-only

### Optimization Tips
- Use GPU acceleration when available
- Choose appropriate model sizes for your hardware
- Enable memory optimization in settings
- Close unused workspaces to free resources

## 🐛 Troubleshooting

### Common Issues

**Model Loading Fails**
- Check available disk space
- Verify internet connection for downloads
- Ensure model format compatibility

**GPU Not Detected**
- Update GPU drivers
- Install CUDA toolkit (NVIDIA) or DirectML (AMD/Intel)
- Check Windows GPU scheduling settings

**Out of Memory Errors**
- Use smaller models
- Enable memory optimization
- Close other applications
- Consider CPU-only inference

**Slow Performance**
- Enable GPU acceleration
- Adjust batch sizes
- Update to latest drivers
- Check background processes

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- **ONNX Runtime** - Cross-platform ML inference
- **Hugging Face** - Model hub and transformers
- **ModernWPF** - Modern UI controls for WPF
- **LM Studio** - Inspiration for local LLM inference

## 📞 Support

- 📧 **Email**: support@lmstudioclone.com
- 💬 **Discord**: [Join our community](https://discord.gg/lmstudioclone)
- 🐛 **Issues**: [GitHub Issues](https://github.com/yourusername/LMStudioClone/issues)
- 📖 **Documentation**: [Wiki](https://github.com/yourusername/LMStudioClone/wiki)

---

**Built with ❤️ using C# .NET and ONNX Runtime**

