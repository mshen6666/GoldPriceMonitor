using System;
using System.IO;

namespace GoldPriceMonitor.Utils;

public static class StartupLogger
{
    private static readonly string LogPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "GoldPriceMonitor",
        "logs",
        "startup.log");

    public static void Log(string message, Exception? ex = null)
    {
        try
        {
            var dir = Path.GetDirectoryName(LogPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            var line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {message}";
            if (ex != null)
            {
                line += Environment.NewLine + ex;
            }
            File.AppendAllText(LogPath, line + Environment.NewLine);
        }
        catch
        {
            // 忽略日志写入失败
        }
    }
}
