namespace MediaPlayer
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        // ========== 控件声明 ==========
        private MenuStrip menuStrip;
        private ToolStripMenuItem fileToolStripMenuItem;
        private ToolStripMenuItem openLocalFilesToolStripMenuItem;
        private ToolStripMenuItem openUrlToolStripMenuItem;
        private ToolStripMenuItem exportPlaylistToolStripMenuItem;
        private ToolStripMenuItem clearPlaylistToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripMenuItem exitToolStripMenuItem;
        private ToolStripMenuItem toolsToolStripMenuItem;
        private ToolStripMenuItem clearCacheToolStripMenuItem;
        private ToolStripMenuItem helpToolStripMenuItem;
        private ToolStripMenuItem aboutToolStripMenuItem;

        private SplitContainer splitContainer;

        // 左侧面板：封面和信息
        private Panel panelLeft;
        private PictureBox pictureBoxCover;
        private Label labelSongTitle;
        private Label labelArtist;

        // 右侧面板：播放列表
        private ListView listViewPlaylist;
        private ColumnHeader columnHeaderTitle;
        private ColumnHeader columnHeaderArtist;
        private ColumnHeader columnHeaderDuration;
        private ColumnHeader columnHeaderSource;
        private Button btnRemoveSong;

        // 底部面板：播放控制
        private Panel panelControls;
        private Button btnPlay;
        private Button btnStop;
        private Button btnPrevious;
        private Button btnNext;
        private Button btnPlayMode;
        private TrackBar trackBarProgress;
        private Label labelCurrentTime;
        private Label labelTotalTime;
        private TrackBar trackBarVolume;
        private Label labelVolume;

        // 状态栏
        private StatusStrip statusStrip;
        private ToolStripStatusLabel toolStripStatusLabel;
        private ToolStripProgressBar toolStripProgressBar;

        /// <summary>
        /// 释放所用资源
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        /// <summary>
        /// 初始化所有UI组件及其布局
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();

            // ========== 主窗体设置 ==========
            Text = "多媒体播放器 - Windows程序设计课程项目";
            ClientSize = new System.Drawing.Size(900, 600);
            MinimumSize = new System.Drawing.Size(800, 500);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = System.Drawing.Color.FromArgb(30, 30, 30);
            ForeColor = System.Drawing.Color.White;
            Font = new System.Drawing.Font("Microsoft YaHei UI", 9F);
            Icon = System.Drawing.SystemIcons.Application;

            // ========== 菜单栏 ==========
            menuStrip = new MenuStrip();
            menuStrip.BackColor = System.Drawing.Color.FromArgb(45, 45, 48);
            menuStrip.ForeColor = System.Drawing.Color.White;

            fileToolStripMenuItem = new ToolStripMenuItem("文件(&F)");
            openLocalFilesToolStripMenuItem = new ToolStripMenuItem("打开本地文件(&O)...");
            openLocalFilesToolStripMenuItem.Click += OpenLocalFilesToolStripMenuItem_Click;
            openUrlToolStripMenuItem = new ToolStripMenuItem("打开网络URL(&U)...");
            openUrlToolStripMenuItem.Click += OpenUrlToolStripMenuItem_Click;
            exportPlaylistToolStripMenuItem = new ToolStripMenuItem("导出播放列表(&E)...");
            exportPlaylistToolStripMenuItem.Click += ExportPlaylistToolStripMenuItem_Click;
            clearPlaylistToolStripMenuItem = new ToolStripMenuItem("清空播放列表");
            clearPlaylistToolStripMenuItem.Click += ClearPlaylistToolStripMenuItem_Click;
            toolStripSeparator1 = new ToolStripSeparator();
            exitToolStripMenuItem = new ToolStripMenuItem("退出(&X)");
            exitToolStripMenuItem.Click += (s, e) => Close();
            fileToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] {
                openLocalFilesToolStripMenuItem, openUrlToolStripMenuItem,
                exportPlaylistToolStripMenuItem, clearPlaylistToolStripMenuItem,
                toolStripSeparator1, exitToolStripMenuItem
            });

            toolsToolStripMenuItem = new ToolStripMenuItem("工具(&T)");
            clearCacheToolStripMenuItem = new ToolStripMenuItem("清空网络缓存");
            clearCacheToolStripMenuItem.Click += ClearCacheToolStripMenuItem_Click;
            toolsToolStripMenuItem.DropDownItems.Add(clearCacheToolStripMenuItem);

            helpToolStripMenuItem = new ToolStripMenuItem("帮助(&H)");
            aboutToolStripMenuItem = new ToolStripMenuItem("关于(&A)...");
            aboutToolStripMenuItem.Click += AboutToolStripMenuItem_Click;
            helpToolStripMenuItem.DropDownItems.Add(aboutToolStripMenuItem);

            menuStrip.Items.AddRange(new ToolStripItem[] {
                fileToolStripMenuItem, toolsToolStripMenuItem, helpToolStripMenuItem
            });
            MainMenuStrip = menuStrip;

            // ========== 状态栏 ==========
            statusStrip = new StatusStrip();
            statusStrip.BackColor = System.Drawing.Color.FromArgb(45, 45, 48);
            statusStrip.ForeColor = System.Drawing.Color.White;
            statusStrip.SizingGrip = false;

            toolStripStatusLabel = new ToolStripStatusLabel("就绪");
            toolStripProgressBar = new ToolStripProgressBar { Width = 150, Visible = false };

            statusStrip.Items.AddRange(new ToolStripItem[] {
                toolStripStatusLabel, toolStripProgressBar
            });

            // ========== 主分割容器 ==========
            splitContainer = new SplitContainer();
            splitContainer.Dock = DockStyle.Fill;
            splitContainer.BackColor = System.Drawing.Color.FromArgb(30, 30, 30);
            splitContainer.SplitterWidth = 2;
            splitContainer.SplitterDistance = 300;

            // ========== 左侧面板：封面 + 歌曲信息 ==========
            panelLeft = new Panel();
            panelLeft.Dock = DockStyle.Fill;
            panelLeft.BackColor = System.Drawing.Color.FromArgb(30, 30, 30);
            panelLeft.Padding = new System.Windows.Forms.Padding(20);

            // 封面图片框
            pictureBoxCover = new PictureBox();
            pictureBoxCover.Size = new System.Drawing.Size(200, 200);
            pictureBoxCover.Location = new System.Drawing.Point(50, 30);
            pictureBoxCover.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBoxCover.BackColor = System.Drawing.Color.FromArgb(50, 50, 55);
            pictureBoxCover.BorderStyle = BorderStyle.FixedSingle;
            // 无封面时显示默认图标
            pictureBoxCover.Paint += (s, e) =>
            {
                if (pictureBoxCover.Image == null)
                {
                    using var font = new System.Drawing.Font("Segoe UI", 48F);
                    e.Graphics.DrawString("🎵", font, Brushes.Gray,
                        new System.Drawing.PointF(60, 50));
                }
            };

            // 歌曲标题
            labelSongTitle = new Label();
            labelSongTitle.AutoSize = false;
            labelSongTitle.Size = new System.Drawing.Size(260, 30);
            labelSongTitle.Location = new System.Drawing.Point(20, 250);
            labelSongTitle.TextAlign = ContentAlignment.MiddleLeft;
            labelSongTitle.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F, System.Drawing.FontStyle.Bold);
            labelSongTitle.ForeColor = System.Drawing.Color.White;
            labelSongTitle.Text = "未选择歌曲";

            // 艺术家
            labelArtist = new Label();
            labelArtist.AutoSize = false;
            labelArtist.Size = new System.Drawing.Size(260, 25);
            labelArtist.Location = new System.Drawing.Point(20, 285);
            labelArtist.TextAlign = ContentAlignment.MiddleLeft;
            labelArtist.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F);
            labelArtist.ForeColor = System.Drawing.Color.Gray;
            labelArtist.Text = "";

            panelLeft.Controls.AddRange(new Control[] {
                pictureBoxCover, labelSongTitle, labelArtist
            });

            // ========== 右侧面板：播放列表 ==========
            var panelRight = new Panel();
            panelRight.Dock = DockStyle.Fill;
            panelRight.BackColor = System.Drawing.Color.FromArgb(30, 30, 30);

            // 列表工具栏
            var panelListToolbar = new Panel();
            panelListToolbar.Dock = DockStyle.Top;
            panelListToolbar.Height = 35;
            panelListToolbar.BackColor = System.Drawing.Color.FromArgb(45, 45, 48);
            panelListToolbar.Padding = new System.Windows.Forms.Padding(5);

            var lblPlaylist = new Label();
            lblPlaylist.Text = "播放列表";
            lblPlaylist.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F, System.Drawing.FontStyle.Bold);
            lblPlaylist.ForeColor = System.Drawing.Color.White;
            lblPlaylist.AutoSize = true;
            lblPlaylist.Location = new System.Drawing.Point(5, 8);

            btnRemoveSong = new Button();
            btnRemoveSong.Text = "删除";
            btnRemoveSong.AutoSize = true;
            btnRemoveSong.Location = new System.Drawing.Point(panelListToolbar.Width - 80, 5);
            btnRemoveSong.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnRemoveSong.BackColor = System.Drawing.Color.FromArgb(70, 70, 75);
            btnRemoveSong.ForeColor = System.Drawing.Color.White;
            btnRemoveSong.FlatStyle = FlatStyle.Flat;
            btnRemoveSong.FlatAppearance.BorderSize = 0;
            btnRemoveSong.Click += BtnRemoveSong_Click;

            panelListToolbar.Controls.AddRange(new Control[] { lblPlaylist, btnRemoveSong });

            // 播放列表 ListView
            listViewPlaylist = new ListView();
            listViewPlaylist.Dock = DockStyle.Fill;
            listViewPlaylist.View = View.Details;
            listViewPlaylist.FullRowSelect = true;
            listViewPlaylist.GridLines = false;
            listViewPlaylist.HideSelection = false;
            listViewPlaylist.MultiSelect = false;
            listViewPlaylist.BackColor = System.Drawing.Color.FromArgb(37, 37, 38);
            listViewPlaylist.ForeColor = System.Drawing.Color.White;
            listViewPlaylist.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F);
            listViewPlaylist.BorderStyle = BorderStyle.None;
            listViewPlaylist.DoubleClick += ListViewPlaylist_DoubleClick;

            // 列标题
            columnHeaderTitle = new ColumnHeader { Text = "标题", Width = 180 };
            columnHeaderArtist = new ColumnHeader { Text = "艺术家", Width = 100 };
            columnHeaderDuration = new ColumnHeader { Text = "时长", Width = 60 };
            columnHeaderSource = new ColumnHeader { Text = "来源", Width = 60 };
            listViewPlaylist.Columns.AddRange(new ColumnHeader[] {
                columnHeaderTitle, columnHeaderArtist, columnHeaderDuration, columnHeaderSource
            });

            // 右键菜单
            var contextMenu = new ContextMenuStrip();
            var removeItem = new ToolStripMenuItem("从播放列表删除");
            removeItem.Click += BtnRemoveSong_Click;
            contextMenu.Items.Add(removeItem);
            listViewPlaylist.ContextMenuStrip = contextMenu;

            panelRight.Controls.AddRange(new Control[] { listViewPlaylist, panelListToolbar });

            // ========== 底部的播放控制面板 ==========
            panelControls = new Panel();
            panelControls.Dock = DockStyle.Bottom;
            panelControls.Height = 100;
            panelControls.BackColor = System.Drawing.Color.FromArgb(45, 45, 48);
            panelControls.Padding = new System.Windows.Forms.Padding(10, 5, 10, 5);

            // 进度条
            trackBarProgress = new TrackBar();
            trackBarProgress.Minimum = 0;
            trackBarProgress.Maximum = 1000;
            trackBarProgress.TickStyle = TickStyle.None;
            trackBarProgress.Width = 500;
            trackBarProgress.Height = 20;
            trackBarProgress.Location = new System.Drawing.Point(120, 10);
            trackBarProgress.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            trackBarProgress.BackColor = System.Drawing.Color.FromArgb(45, 45, 48);
            trackBarProgress.ForeColor = System.Drawing.Color.FromArgb(0, 120, 215);
            trackBarProgress.MouseDown += TrackBarProgress_MouseDown;
            trackBarProgress.MouseUp += TrackBarProgress_MouseUp;

            // 当前时间
            labelCurrentTime = new Label();
            labelCurrentTime.Text = "00:00";
            labelCurrentTime.ForeColor = System.Drawing.Color.Gray;
            labelCurrentTime.Font = new System.Drawing.Font("Consolas", 9F);
            labelCurrentTime.AutoSize = true;
            labelCurrentTime.Location = new System.Drawing.Point(70, 10);
            labelCurrentTime.TextAlign = ContentAlignment.MiddleCenter;

            // 总时间
            labelTotalTime = new Label();
            labelTotalTime.Text = "00:00";
            labelTotalTime.ForeColor = System.Drawing.Color.Gray;
            labelTotalTime.Font = new System.Drawing.Font("Consolas", 9F);
            labelTotalTime.AutoSize = true;
            labelTotalTime.Location = new System.Drawing.Point(630, 10);
            labelTotalTime.TextAlign = ContentAlignment.MiddleCenter;

            // 音量卷标
            labelVolume = new Label();
            labelVolume.Text = "🔊";
            labelVolume.ForeColor = System.Drawing.Color.White;
            labelVolume.AutoSize = true;
            labelVolume.Location = new System.Drawing.Point(680, 10);

            // 音量滑块
            trackBarVolume = new TrackBar();
            trackBarVolume.Minimum = 0;
            trackBarVolume.Maximum = 100;
            trackBarVolume.Value = 80;
            trackBarVolume.TickStyle = TickStyle.None;
            trackBarVolume.Width = 100;
            trackBarVolume.Location = new System.Drawing.Point(710, 5);
            trackBarVolume.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            trackBarVolume.BackColor = System.Drawing.Color.FromArgb(45, 45, 48);
            trackBarVolume.Scroll += TrackBarVolume_Scroll;

            // 播放按钮面板（居中）
            // 上一曲
            btnPrevious = new Button();
            btnPrevious.Text = "⏮";
            btnPrevious.Size = new System.Drawing.Size(40, 35);
            btnPrevious.Location = new System.Drawing.Point(220, 45);
            btnPrevious.FlatStyle = FlatStyle.Flat;
            btnPrevious.FlatAppearance.BorderSize = 0;
            btnPrevious.Font = new System.Drawing.Font("Segoe UI", 12F);
            btnPrevious.ForeColor = System.Drawing.Color.White;
            btnPrevious.Click += BtnPrevious_Click;

            // 播放/暂停
            btnPlay = new Button();
            btnPlay.Text = "▶";
            btnPlay.Size = new System.Drawing.Size(50, 45);
            btnPlay.Location = new System.Drawing.Point(270, 42);
            btnPlay.FlatStyle = FlatStyle.Flat;
            btnPlay.FlatAppearance.BorderSize = 0;
            btnPlay.Font = new System.Drawing.Font("Segoe UI", 16F);
            btnPlay.ForeColor = System.Drawing.Color.FromArgb(0, 120, 215);
            btnPlay.Click += BtnPlay_Click;

            // 停止
            btnStop = new Button();
            btnStop.Text = "⏹";
            btnStop.Size = new System.Drawing.Size(40, 35);
            btnStop.Location = new System.Drawing.Point(330, 45);
            btnStop.FlatStyle = FlatStyle.Flat;
            btnStop.FlatAppearance.BorderSize = 0;
            btnStop.Font = new System.Drawing.Font("Segoe UI", 12F);
            btnStop.ForeColor = System.Drawing.Color.White;
            btnStop.Click += BtnStop_Click;

            // 下一曲
            btnNext = new Button();
            btnNext.Text = "⏭";
            btnNext.Size = new System.Drawing.Size(40, 35);
            btnNext.Location = new System.Drawing.Point(380, 45);
            btnNext.FlatStyle = FlatStyle.Flat;
            btnNext.FlatAppearance.BorderSize = 0;
            btnNext.Font = new System.Drawing.Font("Segoe UI", 12F);
            btnNext.ForeColor = System.Drawing.Color.White;
            btnNext.Click += BtnNext_Click;

            // 播放模式
            btnPlayMode = new Button();
            btnPlayMode.Text = "🔁 顺序播放";
            btnPlayMode.Size = new System.Drawing.Size(110, 30);
            btnPlayMode.Location = new System.Drawing.Point(450, 50);
            btnPlayMode.FlatStyle = FlatStyle.Flat;
            btnPlayMode.FlatAppearance.BorderSize = 0;
            btnPlayMode.Font = new System.Drawing.Font("Microsoft YaHei UI", 8F);
            btnPlayMode.ForeColor = System.Drawing.Color.Gray;
            btnPlayMode.Click += BtnPlayMode_Click;

            // 把控制按钮都放到 panelControls
            panelControls.Controls.AddRange(new Control[] {
                trackBarProgress, labelCurrentTime, labelTotalTime,
                labelVolume, trackBarVolume,
                btnPrevious, btnPlay, btnStop, btnNext, btnPlayMode
            });

            // ========== 主窗体布局 ==========
            splitContainer.Panel1.Controls.Add(panelLeft);
            splitContainer.Panel2.Controls.Add(panelRight);

            Controls.AddRange(new Control[] {
                splitContainer, panelControls, menuStrip, statusStrip
            });

            // 当窗体的MainMenuStrip改变后再设置Padding
            Padding = new System.Windows.Forms.Padding(0, 0, 0, 100);
            panelControls.BringToFront();

            // ========== 初始化结束 ==========
        }
    }
}
