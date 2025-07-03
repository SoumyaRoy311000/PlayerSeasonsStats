using Newtonsoft.Json;
using Serilog;

namespace LMStudioClone.Models
{
    public class ModelCache
    {
        private readonly ILogger _logger = Log.ForContext<ModelCache>();
        private readonly string _cacheDirectory;
        private readonly string _metadataFile;

        public ModelCache()
        {
            _cacheDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), 
                "LMStudioClone", "Models");
            _metadataFile = Path.Combine(_cacheDirectory, "models.json");
            
            Directory.CreateDirectory(_cacheDirectory);
        }

        public string GetModelDirectory(string modelId)
        {
            // Replace invalid path characters
            var safeName = string.Join("_", modelId.Split(Path.GetInvalidFileNameChars()));
            return Path.Combine(_cacheDirectory, safeName);
        }

        public async Task CacheModelInfoAsync(string modelId, HuggingFaceModel modelInfo, List<HuggingFaceFile> files)
        {
            try
            {
                var metadata = await LoadMetadataAsync();
                
                var cachedModel = new CachedModelInfo
                {
                    ModelId = modelId,
                    DisplayName = modelInfo.DisplayName,
                    Author = modelInfo.Author,
                    Downloads = modelInfo.Downloads,
                    Likes = modelInfo.Likes,
                    Tags = modelInfo.Tags,
                    CachedAt = DateTime.UtcNow,
                    Files = files.Select(f => new CachedFileInfo
                    {
                        Path = f.Path,
                        Size = f.Size,
                        IsModelFile = f.IsModelFile
                    }).ToList()
                };

                metadata[modelId] = cachedModel;
                await SaveMetadataAsync(metadata);

                _logger.Information("Cached model info: {ModelId}", modelId);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error caching model info: {ModelId}", modelId);
            }
        }

        public async Task<CachedModelInfo?> GetCachedModelInfoAsync(string modelId)
        {
            try
            {
                var metadata = await LoadMetadataAsync();
                return metadata.TryGetValue(modelId, out var info) ? info : null;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting cached model info: {ModelId}", modelId);
                return null;
            }
        }

        public async Task<List<string>> GetCachedModelsAsync()
        {
            try
            {
                var metadata = await LoadMetadataAsync();
                return metadata.Keys.ToList();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting cached models");
                return new List<string>();
            }
        }

        public async Task<List<CachedModelInfo>> GetAllCachedModelInfoAsync()
        {
            try
            {
                var metadata = await LoadMetadataAsync();
                return metadata.Values.ToList();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting all cached model info");
                return new List<CachedModelInfo>();
            }
        }

        public async Task RemoveModelInfoAsync(string modelId)
        {
            try
            {
                var metadata = await LoadMetadataAsync();
                metadata.Remove(modelId);
                await SaveMetadataAsync(metadata);

                _logger.Information("Removed model info: {ModelId}", modelId);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error removing model info: {ModelId}", modelId);
            }
        }

        public async Task<string?> FindModelFileAsync(string modelId, string[] preferredExtensions = null)
        {
            try
            {
                var modelDir = GetModelDirectory(modelId);
                if (!Directory.Exists(modelDir))
                    return null;

                preferredExtensions ??= new[] { ".onnx", ".bin", ".safetensors", ".pt", ".pth" };

                // Look for files with preferred extensions
                foreach (var ext in preferredExtensions)
                {
                    var files = Directory.GetFiles(modelDir, $"*{ext}", SearchOption.AllDirectories);
                    if (files.Any())
                    {
                        return files.First();
                    }
                }

                // If no preferred files found, return any model file
                var allFiles = Directory.GetFiles(modelDir, "*", SearchOption.AllDirectories);
                var modelFile = allFiles.FirstOrDefault(f => 
                    preferredExtensions.Any(ext => f.EndsWith(ext, StringComparison.OrdinalIgnoreCase)));

                return modelFile;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error finding model file: {ModelId}", modelId);
                return null;
            }
        }

        public async Task CleanupOldModelsAsync(TimeSpan maxAge)
        {
            try
            {
                var metadata = await LoadMetadataAsync();
                var cutoffDate = DateTime.UtcNow - maxAge;
                var modelsToRemove = new List<string>();

                foreach (var kvp in metadata)
                {
                    if (kvp.Value.CachedAt < cutoffDate)
                    {
                        modelsToRemove.Add(kvp.Key);
                    }
                }

                foreach (var modelId in modelsToRemove)
                {
                    var modelDir = GetModelDirectory(modelId);
                    if (Directory.Exists(modelDir))
                    {
                        Directory.Delete(modelDir, true);
                    }
                    metadata.Remove(modelId);
                }

                if (modelsToRemove.Any())
                {
                    await SaveMetadataAsync(metadata);
                    _logger.Information("Cleaned up {Count} old models", modelsToRemove.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error cleaning up old models");
            }
        }

        private async Task<Dictionary<string, CachedModelInfo>> LoadMetadataAsync()
        {
            try
            {
                if (!File.Exists(_metadataFile))
                    return new Dictionary<string, CachedModelInfo>();

                var json = await File.ReadAllTextAsync(_metadataFile);
                return JsonConvert.DeserializeObject<Dictionary<string, CachedModelInfo>>(json) 
                       ?? new Dictionary<string, CachedModelInfo>();
            }
            catch (Exception ex)
            {
                _logger.Warning(ex, "Error loading metadata, starting with empty cache");
                return new Dictionary<string, CachedModelInfo>();
            }
        }

        private async Task SaveMetadataAsync(Dictionary<string, CachedModelInfo> metadata)
        {
            try
            {
                var json = JsonConvert.SerializeObject(metadata, Formatting.Indented);
                await File.WriteAllTextAsync(_metadataFile, json);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error saving metadata");
            }
        }
    }

    public class CachedModelInfo
    {
        public string ModelId { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public int Downloads { get; set; }
        public int Likes { get; set; }
        public List<string> Tags { get; set; } = new();
        public DateTime CachedAt { get; set; }
        public List<CachedFileInfo> Files { get; set; } = new();

        public long TotalSize => Files.Sum(f => f.Size);
        public string SizeDisplay
        {
            get
            {
                var size = TotalSize;
                if (size < 1024) return $"{size} B";
                if (size < 1024 * 1024) return $"{size / 1024.0:F1} KB";
                if (size < 1024 * 1024 * 1024) return $"{size / (1024.0 * 1024):F1} MB";
                return $"{size / (1024.0 * 1024 * 1024):F1} GB";
            }
        }
    }

    public class CachedFileInfo
    {
        public string Path { get; set; } = string.Empty;
        public long Size { get; set; }
        public bool IsModelFile { get; set; }
    }
}

