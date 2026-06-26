using System;
using System.IO;
using NAudio.Wave;
using MediaPlayer.Models;

namespace MediaPlayer.Services
{
    /// <summary>
    /// 播放器服务类 - 封装NAudio库实现音频播放控制
    /// 负责音频文件的加载、播放、暂停、停止、音量控制、进度控制
    /// </summary>
    public class PlayerService : IDisposable
    {
        private IWavePlayer? _waveOutDevice;     // 音频输出设备
        private WaveStream? _audioStream;        // 音频流读取器（基类，支持AudioFileReader和MediaFoundationReader）
        private bool _isPlaying;                 // 是否正在播放
        private bool _isPaused;                  // 是否暂停
        private bool _disposed;                  // 是否已释放资源
        private float _volume = 0.8f;            // 当前音量

        // 当前播放状态事件
        public event Action<double>? PositionChanged;      // 播放位置变化
        public event Action<bool>? PlayStateChanged;       // 播放状态变化
        public event Action? PlaybackStopped;               // 播放停止
        public event Action<string>? ErrorOccurred;         // 错误发生

        public bool IsPlaying => _isPlaying;
        public bool IsPaused => _isPaused;

        /// <summary>
        /// 当前播放时长（秒）
        /// </summary>
        public double CurrentPosition
        {
            get
            {
                if (_audioStream == null) return 0;
                try { return _audioStream.CurrentTime.TotalSeconds; }
                catch { return 0; }
            }
        }

        /// <summary>
        /// 音频总时长（秒）
        /// </summary>
        public double TotalDuration
        {
            get
            {
                if (_audioStream == null) return 0;
                try { return _audioStream.TotalTime.TotalSeconds; }
                catch { return 0; }
            }
        }

        /// <summary>
        /// 初始化输出设备
        /// </summary>
        public void Initialize()
        {
            try
            {
                _waveOutDevice = new WaveOutEvent();
                _waveOutDevice.PlaybackStopped += OnPlaybackStopped;
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke($"音频设备初始化失败：{ex.Message}");
            }
        }

        /// <summary>
        /// 加载并播放本地音频文件
        /// </summary>
        /// <param name="filePath">音频文件路径</param>
        public void LoadAndPlay(string filePath)
        {
            try
            {
                Stop();
                CleanupStream();

                if (!File.Exists(filePath))
                {
                    ErrorOccurred?.Invoke($"文件不存在：{filePath}");
                    return;
                }

                _audioStream = new AudioFileReader(filePath);
                SetupPlayback();
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke($"加载文件失败：{ex.Message}");
            }
        }

        /// <summary>
        /// 加载并播放网络音频流
        /// </summary>
        /// <param name="url">音频流URL</param>
        public void LoadAndPlayUrl(string url)
        {
            try
            {
                Stop();
                CleanupStream();

                // 使用 MediaFoundationReader 支持网络流媒体
                _audioStream = new MediaFoundationReader(url);
                SetupPlayback();
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke($"加载网络流失败：{ex.Message}");
            }
        }

        /// <summary>
        /// 设置播放并开始
        /// 使用音量包装器来实现音量控制
        /// </summary>
        private void SetupPlayback()
        {
            if (_waveOutDevice == null)
            {
                Initialize();
                if (_waveOutDevice == null) return;
            }

            // 使用音量包装器（VolumeWaveProvider16）包装音频流以实现音量控制
            var volumeProvider = new WaveChannel32(_audioStream)
            {
                Volume = _volume,
                PadWithZeroes = false
            };

            _waveOutDevice.Init(volumeProvider);
            _waveOutDevice.Play();
            _isPlaying = true;
            _isPaused = false;
            PlayStateChanged?.Invoke(true);
        }

        /// <summary>
        /// 播放
        /// </summary>
        public void Play()
        {
            if (_waveOutDevice == null) return;

            if (_isPaused && _audioStream != null)
            {
                // 从暂停位置恢复播放
                _waveOutDevice.Play();
                _isPaused = false;
                _isPlaying = true;
                PlayStateChanged?.Invoke(true);
            }
            else if (_audioStream != null && !_isPlaying)
            {
                _waveOutDevice.Play();
                _isPlaying = true;
                PlayStateChanged?.Invoke(true);
            }
        }

        /// <summary>
        /// 暂停播放
        /// </summary>
        public void Pause()
        {
            if (_waveOutDevice == null || !_isPlaying) return;
            _waveOutDevice.Pause();
            _isPaused = true;
            _isPlaying = false;
            PlayStateChanged?.Invoke(false);
        }

        /// <summary>
        /// 停止播放
        /// </summary>
        public void Stop()
        {
            if (_waveOutDevice == null) return;
            _waveOutDevice.Stop();
            _isPlaying = false;
            _isPaused = false;
            PlayStateChanged?.Invoke(false);
        }

        /// <summary>
        /// 设置音量（0.0 ~ 1.0）
        /// </summary>
        public void SetVolume(float volume)
        {
            _volume = Math.Clamp(volume, 0f, 1f);
            // 音量通过WaveChannel32在SetupPlayback时设置，此处保存当前值
        }

        /// <summary>
        /// 跳转到指定位置（秒）
        /// </summary>
        public void Seek(double seconds)
        {
            if (_audioStream == null) return;
            try
            {
                TimeSpan target = TimeSpan.FromSeconds(Math.Clamp(seconds, 0, TotalDuration));
                _audioStream.CurrentTime = target;
                PositionChanged?.Invoke(seconds);
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke($"跳转失败：{ex.Message}");
            }
        }

        /// <summary>
        /// 获取当前播放音频文件的基本信息
        /// </summary>
        public Song? GetCurrentSongInfo(string filePath)
        {
            return new Song
            {
                FilePath = filePath,
                Title = Path.GetFileNameWithoutExtension(filePath),
                Duration = TotalDuration
            };
        }

        private void OnPlaybackStopped(object? sender, StoppedEventArgs e)
        {
            _isPlaying = false;
            _isPaused = false;
            PlayStateChanged?.Invoke(false);
            PlaybackStopped?.Invoke();
        }

        /// <summary>
        /// 清理音频流资源
        /// </summary>
        private void CleanupStream()
        {
            if (_audioStream != null)
            {
                _audioStream.Dispose();
                _audioStream = null;
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            Stop();
            CleanupStream();
            _waveOutDevice?.Dispose();
        }
    }
}
