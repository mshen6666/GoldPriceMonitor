using System.IO;
using System.Text.Json;

namespace GoldPriceMonitor.Utils;

public class AppSettings
{
    private static readonly string SettingsPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "GoldPriceMonitor", "settings.json");

    private const string DefaultApiToken = "";

    public static AppSettings Default { get; } = new()
    {
        ApiToken = DefaultApiToken,
        WindowLeft = 100,
        WindowTop = 100,
        IsWindowVisible = true,
        RefreshIntervalMs = 12000,
        HotKeyEnabled = true
    };

    public string ApiToken { get; set; } = DefaultApiToken;
    public double WindowLeft { get; set; }
    public double WindowTop { get; set; }
    public bool IsWindowVisible { get; set; }
    public int RefreshIntervalMs { get; set; }
    public bool HotKeyEnabled { get; set; }

    public static AppSettings Load()
    {
        try
        {
            if (File.Exists(SettingsPath))
            {
                var json = File.ReadAllText(SettingsPath);
                var settings = JsonSerializer.Deserialize<AppSettings>(json);
                return settings ?? Default;
            }
        }
        catch
        {
            // 读取失败使用默认值
        }

        return Default;
    }

    public void Save()
    {
        try
        {
            var dir = Path.GetDirectoryName(SettingsPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            var json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(SettingsPath, json);
        }
        catch
        {
            // 保存失败
        }
    }
}
