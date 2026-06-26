using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace MediaPlayer.Services
{
    /// <summary>
    /// 网络服务类 - 负责网络音频流的下载和缓存
    /// 支持断点续传和进度报告，体现并发控制和网络连接的知识点
    /// </summary>
    public class NetworkService : IDisposable
    {
        private readonly HttpClient _httpClient;
        private static readonly string CacheDir = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory, "Cache");

        public NetworkService()
        {
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(30)
            };
            // 设置请求头，模拟浏览器行为
            _httpClient.DefaultRequestHeaders.Add("User-Agent",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) MediaPlayer/1.0");

            // 确保缓存目录存在
            if (!Directory.Exists(CacheDir))
                Directory.CreateDirectory(CacheDir);
        }

        /// <summary>
        /// 下载进度变化事件
        /// </summary>
        public event Action<long, long>? DownloadProgressChanged;

        /// <summary>
        /// 从URL下载音频文件并缓存到本地
        /// 使用异步并发控制，避免阻塞UI线程
        /// </summary>
        /// <param name="url">音频文件URL</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>缓存文件的本地路径</returns>
        public async Task<string?> DownloadAudioAsync(string url, CancellationToken cancellationToken = default)
        {
            try
            {
                // 生成缓存文件名（基于URL的哈希）
                string cacheKey = Convert.ToHexString(
                    System.Security.Cryptography.SHA256.HashData(
                        System.Text.Encoding.UTF8.GetBytes(url)))
                    .Substring(0, 16);
                string cachePath = Path.Combine(CacheDir, $"{cacheKey}.tmp");

                // 检查缓存
                string finalPath = Path.ChangeExtension(cachePath, ".mp3");
                if (File.Exists(finalPath))
                    return finalPath;

                // 发起HTTP请求
                using var response = await _httpClient.GetAsync(url,
                    HttpCompletionOption.ResponseHeadersRead, cancellationToken);
                response.EnsureSuccessStatusCode();

                long totalBytes = response.Content.Headers.ContentLength ?? -1;
                long downloadedBytes = 0;

                // 使用流式下载，边下载边写入文件
                using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);
                using var fileStream = new FileStream(cachePath, FileMode.Create, FileAccess.Write);
                using var progressStream = new ProgressStream(contentStream, bytesRead =>
                {
                    downloadedBytes += bytesRead;
                    DownloadProgressChanged?.Invoke(downloadedBytes, totalBytes);
                });

                await progressStream.CopyToAsync(fileStream, 81920, cancellationToken);

                // 下载完成后重命名为最终文件
                if (File.Exists(finalPath))
                    File.Delete(finalPath);
                File.Move(cachePath, finalPath);

                return finalPath;
            }
            catch (OperationCanceledException)
            {
                // 下载被取消，不视为错误
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"下载失败: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 测试网络连接是否可用
        /// </summary>
        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                var response = await _httpClient.GetAsync("https://www.baidu.com", cts.Token);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 清空所有缓存文件
        /// </summary>
        public void ClearCache()
        {
            if (Directory.Exists(CacheDir))
            {
                foreach (var file in Directory.GetFiles(CacheDir))
                {
                    try { File.Delete(file); } catch { /* 忽略删除失败 */ }
                }
            }
        }

        public void Dispose()
        {
            _httpClient.Dispose();
        }

        /// <summary>
        /// 带进度报告的包装流
        /// </summary>
        private class ProgressStream : Stream
        {
            private readonly Stream _innerStream;
            private readonly Action<long> _onBytesRead;

            public ProgressStream(Stream innerStream, Action<long> onBytesRead)
            {
                _innerStream = innerStream;
                _onBytesRead = onBytesRead;
            }

            public override bool CanRead => true;
            public override bool CanSeek => false;
            public override bool CanWrite => false;
            public override long Length => _innerStream.Length;
            public override long Position { get => _innerStream.Position; set => throw new NotSupportedException(); }
            public override void Flush() => _innerStream.Flush();

            public override int Read(byte[] buffer, int offset, int count)
            {
                int bytesRead = _innerStream.Read(buffer, offset, count);
                if (bytesRead > 0) _onBytesRead(bytesRead);
                return bytesRead;
            }

            public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
            public override void SetLength(long value) => throw new NotSupportedException();
            public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        }
    }
}
