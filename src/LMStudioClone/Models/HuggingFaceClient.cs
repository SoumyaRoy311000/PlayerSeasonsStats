using System.Net.Http;
using System.Net.Http.Json;
using Newtonsoft.Json;
using Serilog;

namespace LMStudioClone.Models
{
    public class HuggingFaceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger _logger = Log.ForContext<HuggingFaceClient>();
        private const string BaseUrl = "https://huggingface.co/api";

        public HuggingFaceClient()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "LMStudioClone/1.0");
        }

        public async Task<List<HuggingFaceModel>> SearchModelsAsync(string query = "", string task = "", int limit = 20)
        {
            try
            {
                var url = $"{BaseUrl}/models?search={Uri.EscapeDataString(query)}&limit={limit}";
                if (!string.IsNullOrEmpty(task))
                {
                    url += $"&pipeline_tag={Uri.EscapeDataString(task)}";
                }

                _logger.Information("Searching models: {Url}", url);

                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var models = JsonConvert.DeserializeObject<List<HuggingFaceModel>>(json) ?? new List<HuggingFaceModel>();

                _logger.Information("Found {Count} models", models.Count);
                return models;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error searching models");
                return new List<HuggingFaceModel>();
            }
        }

        public async Task<HuggingFaceModel?> GetModelInfoAsync(string modelId)
        {
            try
            {
                var url = $"{BaseUrl}/models/{Uri.EscapeDataString(modelId)}";
                
                _logger.Information("Getting model info: {ModelId}", modelId);

                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var model = JsonConvert.DeserializeObject<HuggingFaceModel>(json);

                return model;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting model info for {ModelId}", modelId);
                return null;
            }
        }

        public async Task<List<HuggingFaceFile>> GetModelFilesAsync(string modelId)
        {
            try
            {
                var url = $"{BaseUrl}/models/{Uri.EscapeDataString(modelId)}/tree/main";
                
                _logger.Information("Getting model files: {ModelId}", modelId);

                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var files = JsonConvert.DeserializeObject<List<HuggingFaceFile>>(json) ?? new List<HuggingFaceFile>();

                return files;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting model files for {ModelId}", modelId);
                return new List<HuggingFaceFile>();
            }
        }

        public string GetDownloadUrl(string modelId, string filename)
        {
            return $"https://huggingface.co/{modelId}/resolve/main/{filename}";
        }

        public async Task<bool> DownloadFileAsync(string url, string localPath, IProgress<double>? progress = null)
        {
            try
            {
                _logger.Information("Downloading file: {Url} -> {LocalPath}", url, localPath);

                // Ensure directory exists
                var directory = Path.GetDirectoryName(localPath);
                if (!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                using var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
                response.EnsureSuccessStatusCode();

                var totalBytes = response.Content.Headers.ContentLength ?? 0;
                var downloadedBytes = 0L;

                using var contentStream = await response.Content.ReadAsStreamAsync();
                using var fileStream = new FileStream(localPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);

                var buffer = new byte[8192];
                int bytesRead;

                while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    await fileStream.WriteAsync(buffer, 0, bytesRead);
                    downloadedBytes += bytesRead;

                    if (totalBytes > 0 && progress != null)
                    {
                        var progressPercentage = (double)downloadedBytes / totalBytes * 100;
                        progress.Report(progressPercentage);
                    }
                }

                _logger.Information("Download completed: {LocalPath}", localPath);
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error downloading file: {Url}", url);
                return false;
            }
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }

    public class HuggingFaceModel
    {
        [JsonProperty("id")]
        public string Id { get; set; } = string.Empty;

        [JsonProperty("modelId")]
        public string ModelId { get; set; } = string.Empty;

        [JsonProperty("author")]
        public string Author { get; set; } = string.Empty;

        [JsonProperty("sha")]
        public string Sha { get; set; } = string.Empty;

        [JsonProperty("downloads")]
        public int Downloads { get; set; }

        [JsonProperty("likes")]
        public int Likes { get; set; }

        [JsonProperty("pipeline_tag")]
        public string PipelineTag { get; set; } = string.Empty;

        [JsonProperty("tags")]
        public List<string> Tags { get; set; } = new();

        [JsonProperty("createdAt")]
        public DateTime CreatedAt { get; set; }

        [JsonProperty("lastModified")]
        public DateTime LastModified { get; set; }

        [JsonProperty("private")]
        public bool Private { get; set; }

        [JsonProperty("gated")]
        public bool Gated { get; set; }

        [JsonProperty("disabled")]
        public bool Disabled { get; set; }

        [JsonProperty("library_name")]
        public string LibraryName { get; set; } = string.Empty;

        public string DisplayName => !string.IsNullOrEmpty(ModelId) ? ModelId : Id;
        public string SizeDisplay => "Unknown"; // Would need to calculate from files
    }

    public class HuggingFaceFile
    {
        [JsonProperty("path")]
        public string Path { get; set; } = string.Empty;

        [JsonProperty("size")]
        public long Size { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; } = string.Empty;

        [JsonProperty("oid")]
        public string Oid { get; set; } = string.Empty;

        public string SizeDisplay
        {
            get
            {
                if (Size < 1024) return $"{Size} B";
                if (Size < 1024 * 1024) return $"{Size / 1024.0:F1} KB";
                if (Size < 1024 * 1024 * 1024) return $"{Size / (1024.0 * 1024):F1} MB";
                return $"{Size / (1024.0 * 1024 * 1024):F1} GB";
            }
        }

        public bool IsModelFile => Path.EndsWith(".onnx") || Path.EndsWith(".bin") || 
                                  Path.EndsWith(".safetensors") || Path.EndsWith(".pt") || 
                                  Path.EndsWith(".pth");
    }
}

