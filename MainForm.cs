using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using MediaPlayer.Models;
using MediaPlayer.Services;

namespace MediaPlayer
{
    /// <summary>
    /// 多媒体播放器主窗体
    /// 整合了音频播放、播放列表管理、网络下载等功能
    /// </summary>
    public partial class MainForm : Form
    {
        private readonly PlayerService _playerService;
        private readonly DatabaseService _databaseService;
        private readonly NetworkService _networkService;
        private System.Windows.Forms.Timer? _positionTimer;
        private bool _isUserDraggingTrackBar;
        private CancellationTokenSource? _downloadCts;

        // 播放模式
        private enum PlayMode { Sequential, RepeatOne, Shuffle }
        private PlayMode _currentPlayMode = PlayMode.Sequential;
        private Random _random = new Random();

        public MainForm()
        {
            InitializeComponent();

            // 初始化各服务组件
            _playerService = new PlayerService();
            _databaseService = new DatabaseService();
            _networkService = new NetworkService();

            // 注册播放器事件
            _playerService.PositionChanged += OnPositionChanged;
            _playerService.PlayStateChanged += OnPlayStateChanged;
            _playerService.PlaybackStopped += OnPlaybackStopped;
            _playerService.ErrorOccurred += OnPlayerError;
            _networkService.DownloadProgressChanged += OnDownloadProgressChanged;

            // 初始化数据库和UI
            InitializeApp();
        }

        /// <summary>
        /// 初始化应用程序：创建数据库、加载播放列表、设置定时器
        /// </summary>
        private void InitializeApp()
        {
            try
            {
                _databaseService.Initialize();
                LoadPlaylistFromDatabase();

                // 设置定时器用于更新进度条
                _positionTimer = new System.Windows.Forms.Timer { Interval = 500 };
                _positionTimer.Tick += PositionTimer_Tick;
                _positionTimer.Start();

                // 设置播放模式图标
                UpdatePlayModeButton();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"初始化失败：{ex.Message}", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 从数据库加载播放列表到ListView
        /// </summary>
        private void LoadPlaylistFromDatabase()
        {
            listViewPlaylist.Items.Clear();
            var songs = _databaseService.GetAllSongs();

            foreach (var song in songs)
            {
                AddSongToListView(song);
            }

            // 更新状态
            UpdateStatusBar();
        }

        /// <summary>
        /// 将Song对象添加到ListView控件
        /// </summary>
        private void AddSongToListView(Song song)
        {
            var item = new ListViewItem(song.Title)
            {
                Tag = song,
                SubItems = {
                    song.Artist,
                    song.DurationFormatted,
                    song.IsLocal ? "本地" : "网络"
                }
            };
            listViewPlaylist.Items.Add(item);
        }

        /// <summary>
        /// 更新状态栏信息
        /// </summary>
        private void UpdateStatusBar()
        {
            toolStripStatusLabel.Text = $"歌曲数: {listViewPlaylist.Items.Count}";
        }

        /// <summary>
        /// 定时器触发：更新进度条和当前时间标签
        /// </summary>
        private void PositionTimer_Tick(object? sender, EventArgs e)
        {
            if (_playerService.IsPlaying && !_isUserDraggingTrackBar)
            {
                double current = _playerService.CurrentPosition;
                double total = _playerService.TotalDuration;

                trackBarProgress.Value = total > 0
                    ? (int)(current / total * trackBarProgress.Maximum)
                    : 0;

                labelCurrentTime.Text = TimeSpan.FromSeconds(current).ToString(@"mm\:ss");
                labelTotalTime.Text = TimeSpan.FromSeconds(total).ToString(@"mm\:ss");
            }
        }

        /// <summary>
        /// 播放位置变化事件处理
        /// </summary>
        private void OnPositionChanged(double position)
        {
            // 可在UI上更新位置信息
        }

        /// <summary>
        /// 播放状态变化事件处理
        /// </summary>
        private void OnPlayStateChanged(bool playing)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => UpdatePlayButtonState(playing)));
            }
            else
            {
                UpdatePlayButtonState(playing);
            }
        }

        /// <summary>
        /// 更新播放按钮的图标状态
        /// </summary>
        private void UpdatePlayButtonState(bool playing)
        {
            if (playing)
                btnPlay.ImageIndex = 1;  // 暂停图标
            else
                btnPlay.ImageIndex = 0;  // 播放图标
        }

        /// <summary>
        /// 播放停止事件处理
        /// </summary>
        private void OnPlaybackStopped()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() =>
                {
                    trackBarProgress.Value = 0;
                    labelCurrentTime.Text = "00:00";
                    UpdatePlayButtonState(false);
                }));
            }
        }

        /// <summary>
        /// 播放器错误事件处理
        /// </summary>
        private void OnPlayerError(string errorMessage)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() =>
                    MessageBox.Show(errorMessage, "播放错误",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning)));
            }
        }

        /// <summary>
        /// 网络下载进度事件处理
        /// </summary>
        private void OnDownloadProgressChanged(long downloaded, long total)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() =>
                {
                    if (total > 0)
                    {
                        int percent = (int)(downloaded * 100 / total);
                        toolStripProgressBar.Value = Math.Min(percent, 100);
                        toolStripStatusLabel.Text = $"下载中: {percent}% ({FormatSize(downloaded)}/{FormatSize(total)})";
                    }
                    else
                    {
                        toolStripStatusLabel.Text = $"下载中: {FormatSize(downloaded)}";
                    }
                }));
            }
        }

        /// <summary>
        /// 格式化文件大小显示
        /// </summary>
        private static string FormatSize(long bytes)
        {
            if (bytes < 1024) return $"{bytes}B";
            if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F1}KB";
            return $"{bytes / (1024.0 * 1024.0):F1}MB";
        }

        // ==================== 用户交互事件处理 ====================

        #region 播放控制

        /// <summary>
        /// 播放/暂停按钮点击
        /// </summary>
        private void BtnPlay_Click(object? sender, EventArgs e)
        {
            if (_playerService.IsPlaying)
                _playerService.Pause();
            else if (_playerService.IsPaused)
                _playerService.Play();
            else if (listViewPlaylist.SelectedItems.Count > 0)
                PlaySelectedSong(listViewPlaylist.SelectedItems[0]);
        }

        /// <summary>
        /// 停止按钮点击
        /// </summary>
        private void BtnStop_Click(object? sender, EventArgs e)
        {
            _playerService.Stop();
        }

        /// <summary>
        /// 上一曲按钮点击
        /// </summary>
        private void BtnPrevious_Click(object? sender, EventArgs e)
        {
            PlayAdjacentSong(-1);
        }

        /// <summary>
        /// 下一曲按钮点击
        /// </summary>
        private void BtnNext_Click(object? sender, EventArgs e)
        {
            PlayAdjacentSong(1);
        }

        /// <summary>
        /// 播放相邻歌曲（上/下一首）
        /// </summary>
        private void PlayAdjacentSong(int direction)
        {
            if (listViewPlaylist.Items.Count == 0) return;

            int currentIndex = -1;
            if (listViewPlaylist.SelectedItems.Count > 0)
                currentIndex = listViewPlaylist.SelectedItems[0].Index;

            int nextIndex;
            switch (_currentPlayMode)
            {
                case PlayMode.Shuffle:
                    nextIndex = _random.Next(listViewPlaylist.Items.Count);
                    break;
                default:
                    nextIndex = (currentIndex + direction + listViewPlaylist.Items.Count)
                                % listViewPlaylist.Items.Count;
                    break;
            }

            listViewPlaylist.Items[nextIndex].Selected = true;
            listViewPlaylist.EnsureVisible(nextIndex);
            PlaySelectedSong(listViewPlaylist.Items[nextIndex]);
        }

        /// <summary>
        /// 播放列表中选中的歌曲
        /// </summary>
        private void PlaySelectedSong(ListViewItem item)
        {
            if (item?.Tag is not Song song) return;

            // 更新UI显示信息
            labelSongTitle.Text = song.DisplayName;
            if (string.IsNullOrEmpty(song.Artist))
                labelArtist.Text = "未知艺术家";
            else
                labelArtist.Text = song.Artist;

            // 显示封面图片（图像处理）
            if (song.CoverArt != null && song.CoverArt.Length > 0)
            {
                try
                {
                    using var ms = new MemoryStream(song.CoverArt);
                    pictureBoxCover.Image?.Dispose();
                    pictureBoxCover.Image = Image.FromStream(ms);
                }
                catch
                {
                    pictureBoxCover.Image = null;
                }
            }
            else
            {
                pictureBoxCover.Image = null;
            }

            // 播放音频
            if (song.IsLocal && File.Exists(song.FilePath))
            {
                _playerService.LoadAndPlay(song.FilePath);
            }
            else if (!string.IsNullOrEmpty(song.Url))
            {
                // 网络流媒体播放
                _playerService.LoadAndPlayUrl(song.Url);
            }
        }

        /// <summary>
        /// 进度条拖拽开始
        /// </summary>
        private void TrackBarProgress_MouseDown(object? sender, MouseEventArgs e)
        {
            _isUserDraggingTrackBar = true;
        }

        /// <summary>
        /// 进度条拖拽结束 - 跳转到指定位置
        /// </summary>
        private void TrackBarProgress_MouseUp(object? sender, MouseEventArgs e)
        {
            _isUserDraggingTrackBar = false;
            double total = _playerService.TotalDuration;
            double target = total * trackBarProgress.Value / trackBarProgress.Maximum;
            _playerService.Seek(target);
        }

        /// <summary>
        /// 音量滑块值变化
        /// </summary>
        private void TrackBarVolume_Scroll(object? sender, EventArgs e)
        {
            float volume = trackBarVolume.Value / 100f;
            _playerService.SetVolume(volume);
        }

        #endregion

        #region 播放模式

        /// <summary>
        /// 切换播放模式按钮点击
        /// </summary>
        private void BtnPlayMode_Click(object? sender, EventArgs e)
        {
            _currentPlayMode = (PlayMode)(((int)_currentPlayMode + 1) % 3);
            UpdatePlayModeButton();
        }

        /// <summary>
        /// 更新播放模式按钮图标
        /// </summary>
        private void UpdatePlayModeButton()
        {
            string text = _currentPlayMode switch
            {
                PlayMode.RepeatOne => "🔂 单曲循环",
                PlayMode.Shuffle => "🔀 随机播放",
                _ => "🔁 顺序播放"
            };
            btnPlayMode.Text = text;
        }

        #endregion

        #region 文件操作

        /// <summary>
        /// 打开本地文件菜单项点击
        /// </summary>
        private async void OpenLocalFilesToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            using var openDialog = new OpenFileDialog
            {
                Multiselect = true,
                Filter = "音频文件|*.mp3;*.wav;*.wma;*.aac;*.flac;*.ogg|所有文件|*.*",
                Title = "选择音频文件"
            };

            if (openDialog.ShowDialog() != DialogResult.OK) return;

            foreach (var filePath in openDialog.FileNames)
            {
                var song = new Song
                {
                    Title = Path.GetFileNameWithoutExtension(filePath),
                    FilePath = filePath,
                    IsLocal = true,
                    Duration = 0  // 将在播放时获取准确时长
                };

                // 尝试提取封面（使用文件流读取专辑封面 - 简化处理）
                try
                {
                    // 简化的封面提取：检查同目录下同名图片文件
                    string dir = Path.GetDirectoryName(filePath) ?? "";
                    string fileNameWithoutExt = Path.GetFileNameWithoutExtension(filePath);
                    string[] imageExts = { ".jpg", ".jpeg", ".png", ".bmp" };

                    foreach (var ext in imageExts)
                    {
                        string imagePath = Path.Combine(dir, fileNameWithoutExt + ext);
                        if (File.Exists(imagePath))
                        {
                            song.CoverArt = await File.ReadAllBytesAsync(imagePath);
                            break;
                        }
                    }
                }
                catch
                {
                    // 封面提取失败不影响主功能
                }

                int id = _databaseService.AddSong(song);
                song.Id = id;
                AddSongToListView(song);
            }

            UpdateStatusBar();
        }

        /// <summary>
        /// 打开网络URL菜单项点击
        /// </summary>
        private async void OpenUrlToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            using var dialog = new InputDialog("打开网络音频",
                "请输入音频文件的网络URL地址：\n(支持 http/https 流媒体地址)",
                "https://");
            if (dialog.ShowDialog(this) != DialogResult.OK) return;
            string url = dialog.InputText;

            if (string.IsNullOrWhiteSpace(url)) return;

            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) ||
                (uri.Scheme != "http" && uri.Scheme != "https"))
            {
                MessageBox.Show("请输入有效的HTTP/HTTPS地址", "提示",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 先尝试下载缓存，同时播放流媒体
            var song = new Song
            {
                Title = Path.GetFileNameWithoutExtension(uri.LocalPath),
                Url = url,
                IsLocal = false,
                Artist = "网络音频"
            };

            int id = _databaseService.AddSong(song);
            song.Id = id;
            AddSongToListView(song);
            UpdateStatusBar();

            // 后台下载缓存
            _downloadCts?.Cancel();
            _downloadCts = new CancellationTokenSource();
            string? cachedPath = await _networkService.DownloadAudioAsync(url, _downloadCts.Token);

            if (cachedPath != null)
            {
                // 更新为本地播放
                song.IsLocal = true;
                song.FilePath = cachedPath;
                // 刷新ListView显示
                foreach (ListViewItem item in listViewPlaylist.Items)
                {
                    if (item.Tag is Song s && s.Id == song.Id)
                    {
                        item.SubItems[3].Text = "本地(已缓存)";
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// 从列表移除选中歌曲
        /// </summary>
        private void BtnRemoveSong_Click(object? sender, EventArgs e)
        {
            if (listViewPlaylist.SelectedItems.Count == 0) return;

            var selected = listViewPlaylist.SelectedItems[0];
            if (selected.Tag is Song song)
            {
                _databaseService.DeleteSong(song.Id);
                listViewPlaylist.Items.Remove(selected);
                UpdateStatusBar();
            }
        }

        /// <summary>
        /// 清空播放列表
        /// </summary>
        private void ClearPlaylistToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            if (MessageBox.Show("确定要清空播放列表吗？", "确认",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            _playerService.Stop();
            _databaseService.ClearAllSongs();
            listViewPlaylist.Items.Clear();
            UpdateStatusBar();
        }

        #endregion

        #region 列表交互

        /// <summary>
        /// 双击播放列表中的歌曲
        /// </summary>
        private void ListViewPlaylist_DoubleClick(object? sender, EventArgs e)
        {
            if (listViewPlaylist.SelectedItems.Count > 0)
            {
                PlaySelectedSong(listViewPlaylist.SelectedItems[0]);
            }
        }

        /// <summary>
        /// 导出播放列表为M3U格式
        /// </summary>
        private void ExportPlaylistToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            using var saveDialog = new SaveFileDialog
            {
                Filter = "M3U播放列表|*.m3u",
                Title = "导出播放列表"
            };

            if (saveDialog.ShowDialog() != DialogResult.OK) return;

            using var writer = new StreamWriter(saveDialog.FileName);
            writer.WriteLine("#EXTM3U");
            foreach (ListViewItem item in listViewPlaylist.Items)
            {
                if (item.Tag is Song song)
                {
                    writer.WriteLine($"#EXTINF:{song.Duration},{song.DisplayName}");
                    writer.WriteLine(song.IsLocal ? song.FilePath : song.Url);
                }
            }
            MessageBox.Show("播放列表导出成功！", "提示",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// 清空缓存菜单项点击
        /// </summary>
        private void ClearCacheToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            _networkService.ClearCache();
            MessageBox.Show("网络缓存已清空", "提示",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// 关于菜单项点击
        /// </summary>
        private void AboutToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            MessageBox.Show(
                "多媒体播放器 v1.0\n\n" +
                "基于 C# WinForms + NAudio + SQLite 开发\n" +
                "支持本地音频播放、网络流媒体播放、播放列表管理\n\n" +
                "—— Windows程序设计课程项目",
                "关于", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        #endregion

        #region 窗口关闭

        /// <summary>
        /// 窗体关闭时释放资源
        /// </summary>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _positionTimer?.Stop();
            _positionTimer?.Dispose();
            _playerService.Dispose();
            _databaseService.Dispose();
            _networkService.Dispose();
            _downloadCts?.Cancel();
            _downloadCts?.Dispose();
            base.OnFormClosing(e);
        }

        #endregion
    }
}
