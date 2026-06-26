using System;
using System.Drawing;
using System.IO;

namespace MediaPlayer.Models
{
    /// <summary>
    /// 歌曲数据模型 - 表示一首歌曲的全部信息
    /// </summary>
    public class Song
    {
        public int Id { get; set; }                 // 数据库主键
        public string Title { get; set; } = "";      // 歌曲标题
        public string Artist { get; set; } = "";     // 艺术家
        public string Album { get; set; } = "";      // 专辑
        public string FilePath { get; set; } = "";   // 本地文件路径（如果是本地文件）
        public string Url { get; set; } = "";        // 网络URL（如果是网络流媒体）
        public double Duration { get; set; }          // 时长（秒）
        public bool IsLocal { get; set; } = true;     // 是否为本地文件
        public byte[]? CoverArt { get; set; }         // 封面图片数据（二进制）

        /// <summary>
        /// 获取歌曲显示名称："艺术家 - 标题"格式，如果为空则显示文件名
        /// </summary>
        public string DisplayName
        {
            get
            {
                if (!string.IsNullOrEmpty(Artist) && !string.IsNullOrEmpty(Title))
                    return $"{Artist} - {Title}";
                if (!string.IsNullOrEmpty(Title))
                    return Title;
                if (!string.IsNullOrEmpty(FilePath))
                    return Path.GetFileName(FilePath);
                if (!string.IsNullOrEmpty(Url))
                    return Path.GetFileName(new Uri(Url).LocalPath);
                return "未知歌曲";
            }
        }

        /// <summary>
        /// 将时长格式化为 mm:ss 格式
        /// </summary>
        public string DurationFormatted
        {
            get
            {
                var ts = TimeSpan.FromSeconds(Duration);
                return ts.TotalHours >= 1
                    ? ts.ToString(@"h\:mm\:ss")
                    : ts.ToString(@"mm\:ss");
            }
        }

        /// <summary>
        /// 从二进制数据加载封面图片
        /// </summary>
        public Image? GetCoverImage()
        {
            if (CoverArt == null || CoverArt.Length == 0)
                return null;
            try
            {
                using var ms = new MemoryStream(CoverArt);
                return Image.FromStream(ms);
            }
            catch
            {
                return null;
            }
        }

        public override string ToString() => DisplayName;
    }
}
