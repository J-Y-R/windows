using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using MediaPlayer.Models;

namespace MediaPlayer.Services
{
    /// <summary>
    /// 数据库服务类 - 管理SQLite数据库操作
    /// 用于持久化存储播放列表和歌曲信息
    /// </summary>
    public class DatabaseService : IDisposable
    {
        private readonly string _connectionString;
        private SQLiteConnection? _connection;

        /// <summary>
        /// 数据库文件路径：与应用程序同目录下的 MediaPlayer.db
        /// </summary>
        public DatabaseService()
        {
            string dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MediaPlayer.db");
            _connectionString = $"Data Source={dbPath};Version=3;";
        }

        /// <summary>
        /// 初始化数据库连接并创建表结构
        /// </summary>
        public void Initialize()
        {
            _connection = new SQLiteConnection(_connectionString);
            _connection.Open();

            // 创建歌曲表（如果不存在）
            string createTableSql = @"
                CREATE TABLE IF NOT EXISTS Songs (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Title TEXT NOT NULL,
                    Artist TEXT DEFAULT '',
                    Album TEXT DEFAULT '',
                    FilePath TEXT DEFAULT '',
                    Url TEXT DEFAULT '',
                    Duration REAL DEFAULT 0,
                    IsLocal INTEGER DEFAULT 1,
                    CoverArt BLOB DEFAULT NULL,
                    PlaylistOrder INTEGER DEFAULT 0
                )";

            using var command = new SQLiteCommand(createTableSql, _connection);
            command.ExecuteNonQuery();
        }

        /// <summary>
        /// 获取所有歌曲
        /// </summary>
        public List<Song> GetAllSongs()
        {
            var songs = new List<Song>();
            if (_connection == null) return songs;

            string sql = "SELECT * FROM Songs ORDER BY PlaylistOrder ASC";
            using var command = new SQLiteCommand(sql, _connection);
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                songs.Add(MapReaderToSong(reader));
            }
            return songs;
        }

        /// <summary>
        /// 添加一首歌到数据库
        /// </summary>
        public int AddSong(Song song)
        {
            if (_connection == null) return -1;

            // 获取当前最大排序序号
            string maxOrderSql = "SELECT COALESCE(MAX(PlaylistOrder), 0) + 1 FROM Songs";
            using var maxCmd = new SQLiteCommand(maxOrderSql, _connection);
            int nextOrder = Convert.ToInt32(maxCmd.ExecuteScalar());

            string sql = @"
                INSERT INTO Songs (Title, Artist, Album, FilePath, Url, Duration, IsLocal, CoverArt, PlaylistOrder)
                VALUES (@Title, @Artist, @Album, @FilePath, @Url, @Duration, @IsLocal, @CoverArt, @Order)";

            using var command = new SQLiteCommand(sql, _connection);
            command.Parameters.AddWithValue("@Title", song.Title);
            command.Parameters.AddWithValue("@Artist", song.Artist);
            command.Parameters.AddWithValue("@Album", song.Album);
            command.Parameters.AddWithValue("@FilePath", song.FilePath);
            command.Parameters.AddWithValue("@Url", song.Url);
            command.Parameters.AddWithValue("@Duration", song.Duration);
            command.Parameters.AddWithValue("@IsLocal", song.IsLocal ? 1 : 0);
            command.Parameters.AddWithValue("@CoverArt", song.CoverArt ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Order", nextOrder);

            command.ExecuteNonQuery();
            return (int)_connection.LastInsertRowId;
        }

        /// <summary>
        /// 删除指定歌曲
        /// </summary>
        public void DeleteSong(int songId)
        {
            if (_connection == null) return;
            string sql = "DELETE FROM Songs WHERE Id = @Id";
            using var command = new SQLiteCommand(sql, _connection);
            command.Parameters.AddWithValue("@Id", songId);
            command.ExecuteNonQuery();
        }

        /// <summary>
        /// 清空播放列表
        /// </summary>
        public void ClearAllSongs()
        {
            if (_connection == null) return;
            string sql = "DELETE FROM Songs";
            using var command = new SQLiteCommand(sql, _connection);
            command.ExecuteNonQuery();
        }

        /// <summary>
        /// 更新歌曲排序顺序
        /// </summary>
        public void UpdatePlaylistOrder(List<int> songIds)
        {
            if (_connection == null) return;
            for (int i = 0; i < songIds.Count; i++)
            {
                string sql = "UPDATE Songs SET PlaylistOrder = @Order WHERE Id = @Id";
                using var command = new SQLiteCommand(sql, _connection);
                command.Parameters.AddWithValue("@Order", i);
                command.Parameters.AddWithValue("@Id", songIds[i]);
                command.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// 将SQLite数据行映射为Song对象
        /// </summary>
        private static Song MapReaderToSong(SQLiteDataReader reader)
        {
            var song = new Song
            {
                Id = Convert.ToInt32(reader["Id"]),
                Title = reader["Title"]?.ToString() ?? "",
                Artist = reader["Artist"]?.ToString() ?? "",
                Album = reader["Album"]?.ToString() ?? "",
                FilePath = reader["FilePath"]?.ToString() ?? "",
                Url = reader["Url"]?.ToString() ?? "",
                Duration = Convert.ToDouble(reader["Duration"]),
                IsLocal = Convert.ToInt32(reader["IsLocal"]) == 1
            };

            // 处理封面图片二进制数据
            if (reader["CoverArt"] != DBNull.Value)
            {
                long size = reader.GetBytes(reader.GetOrdinal("CoverArt"), 0, null, 0, 0);
                if (size > 0)
                {
                    song.CoverArt = new byte[size];
                    reader.GetBytes(reader.GetOrdinal("CoverArt"), 0, song.CoverArt, 0, (int)size);
                }
            }

            return song;
        }

        public void Dispose()
        {
            _connection?.Close();
            _connection?.Dispose();
        }
    }
}
