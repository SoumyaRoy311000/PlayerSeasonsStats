using Serilog;

namespace LMStudioClone.Models
{
    public class ModelDownloader
    {
        private readonly HuggingFaceClient _huggingFaceClient;
        private readonly ModelCache _modelCache;
        private readonly ILogger _logger = Log.ForContext<ModelDownloader>();

        public ModelDownloader(HuggingFaceClient huggingFaceClient, ModelCache modelCache)
        {
            _huggingFaceClient = huggingFaceClient;
            _modelCache = modelCache;
        }

        public async Task<bool> DownloadModelAsync(string modelId, IProgress<DownloadProgress>? progress = null)
        {
            try
            {
                _logger.Information("Starting download for model: {ModelId}", modelId);

                // Get model information
                var modelInfo = await _huggingFaceClient.GetModelInfoAsync(modelId);
                if (modelInfo == null)
                {
                    _logger.Error("Could not get model information for {ModelId}", modelId);
                    return false;
                }

                // Get model files
                var files = await _huggingFaceClient.GetModelFilesAsync(modelId);
                var modelFiles = files.Where(f => f.IsModelFile).ToList();

                if (!modelFiles.Any())
                {
                    _logger.Warning("No model files found for {ModelId}", modelId);
                    return false;
                }

                // Create model directory
                var modelDir = _modelCache.GetModelDirectory(modelId);
                Directory.CreateDirectory(modelDir);

                // Download files
                var totalFiles = modelFiles.Count;
                var completedFiles = 0;

                foreach (var file in modelFiles)
                {
                    var downloadUrl = _huggingFaceClient.GetDownloadUrl(modelId, file.Path);
                    var localPath = Path.Combine(modelDir, file.Path);

                    // Ensure subdirectory exists
                    var fileDir = Path.GetDirectoryName(localPath);
                    if (!string.IsNullOrEmpty(fileDir))
                    {
                        Directory.CreateDirectory(fileDir);
                    }

                    // Download file with progress tracking
                    var fileProgress = new Progress<double>(percent =>
                    {
                        var overallProgress = (completedFiles + percent / 100.0) / totalFiles * 100.0;
                        progress?.Report(new DownloadProgress
                        {
                            OverallProgress = overallProgress,
                            CurrentFile = file.Path,
                            FileProgress = percent,
                            CompletedFiles = completedFiles,
                            TotalFiles = totalFiles
                        });
                    });

                    var success = await _huggingFaceClient.DownloadFileAsync(downloadUrl, localPath, fileProgress);
                    if (!success)
                    {
                        _logger.Error("Failed to download file: {FilePath}", file.Path);
                        return false;
                    }

                    completedFiles++;
                }

                // Cache model information
                await _modelCache.CacheModelInfoAsync(modelId, modelInfo, modelFiles);

                _logger.Information("Successfully downloaded model: {ModelId}", modelId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error downloading model: {ModelId}", modelId);
                return false;
            }
        }

        public async Task<List<string>> GetDownloadedModelsAsync()
        {
            return await _modelCache.GetCachedModelsAsync();
        }

        public async Task<bool> DeleteModelAsync(string modelId)
        {
            try
            {
                var modelDir = _modelCache.GetModelDirectory(modelId);
                if (Directory.Exists(modelDir))
                {
                    Directory.Delete(modelDir, true);
                    await _modelCache.RemoveModelInfoAsync(modelId);
                    _logger.Information("Deleted model: {ModelId}", modelId);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error deleting model: {ModelId}", modelId);
                return false;
            }
        }

        public async Task<long> GetModelSizeAsync(string modelId)
        {
            try
            {
                var modelDir = _modelCache.GetModelDirectory(modelId);
                if (!Directory.Exists(modelDir))
                    return 0;

                var files = Directory.GetFiles(modelDir, "*", SearchOption.AllDirectories);
                return files.Sum(file => new FileInfo(file).Length);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error calculating model size: {ModelId}", modelId);
                return 0;
            }
        }
    }

    public class DownloadProgress
    {
        public double OverallProgress { get; set; }
        public string CurrentFile { get; set; } = string.Empty;
        public double FileProgress { get; set; }
        public int CompletedFiles { get; set; }
        public int TotalFiles { get; set; }
    }
}

