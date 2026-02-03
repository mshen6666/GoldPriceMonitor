using GoldPriceMonitor.Utils;
using System.Windows;
using System.Windows.Navigation;

namespace GoldPriceMonitor.Views;

public partial class SettingsWindow : Window
{
    private readonly AppSettings _settings;

    public string ApiToken
    {
        get => _settings.ApiToken;
        set => _settings.ApiToken = value;
    }

    public SettingsWindow()
    {
        InitializeComponent();

        _settings = AppSettings.Load();
        ApiTokenTextBox.Text = _settings.ApiToken;
        RefreshIntervalTextBox.Text = (_settings.RefreshIntervalMs / 1000).ToString();

        DataContext = this;
    }

    private void Window_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
        {
            DragMove();
        }
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        // 验证刷新间隔
        if (int.TryParse(RefreshIntervalTextBox.Text, out int interval) && interval >= 1)
        {
            _settings.RefreshIntervalMs = interval * 1000;
        }
        else
        {
            _settings.RefreshIntervalMs = 5000;
        }

        _settings.ApiToken = ApiTokenTextBox.Text.Trim();
        _settings.Save();

        System.Windows.MessageBox.Show("设置已保存", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
        Close();
    }

    private void ApiTokenTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        // 实时更新
    }

    private void RefreshIntervalTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        // 实时更新
    }

    private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
    {
        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
        {
            FileName = e.Uri.AbsoluteUri,
            UseShellExecute = true
        });
    }
}
