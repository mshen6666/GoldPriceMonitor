using GoldPriceMonitor.Models;
using GoldPriceMonitor.Services;
using GoldPriceMonitor.Utils;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace GoldPriceMonitor.ViewModels;

public class MainViewModel : INotifyPropertyChanged
{
    private readonly GoldApiService _apiService;
    private readonly AppSettings _settings;
    private readonly CancellationTokenSource _cts;
    private decimal _previousGoldPrice;
    private decimal _previousSilverPrice;

    public DisplayPrice LondonGold { get; private set; } = new() { Name = "伦敦金" };
    public DisplayPrice LondonSilver { get; private set; } = new() { Name = "伦敦银" };
    public DisplayPrice CnyGold { get; private set; } = new() { Name = "人民币金" };

    public ObservableCollection<PricePoint> PriceHistory { get; } = new();

    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    private string _lastUpdateTime = string.Empty;
    public string LastUpdateTime
    {
        get => _lastUpdateTime;
        set => SetProperty(ref _lastUpdateTime, value);
    }

    private string _statusMessage = "正在加载数据...";
    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public MainViewModel()
    {
        _apiService = new GoldApiService(AppSettings.Default.ApiToken);
        _settings = AppSettings.Load();
        _cts = new CancellationTokenSource();

        if (_settings.ApiToken != AppSettings.Default.ApiToken)
        {
            _apiService = new GoldApiService(_settings.ApiToken);
        }

        // 设置初始值
        LondonGold.FormattedPrice = "---";
        LondonGold.FormattedChange = "---";
        LondonSilver.FormattedPrice = "---";
        LondonSilver.FormattedChange = "---";
        CnyGold.FormattedPrice = "---";
        CnyGold.FormattedChange = "---";
        LastUpdateTime = "等待数据...";
    }

    public async Task StartAutoRefreshAsync()
    {
        while (!_cts.Token.IsCancellationRequested)
        {
            try
            {
                await RefreshPricesAsync();
            }
            catch (Exception ex)
            {
                StatusMessage = $"错误: {ex.Message}";
            }

            try
            {
                await Task.Delay(_settings.RefreshIntervalMs, _cts.Token);
            }
            catch (TaskCanceledException)
            {
                break;
            }
        }
    }

    public async Task RefreshPricesAsync()
    {
        IsLoading = true;

        var goldTask = _apiService.GetLondonGoldAsync();
        var silverTask = _apiService.GetLondonSilverAsync();

        await Task.WhenAll(goldTask, silverTask);

        var gold = await goldTask;
        var silver = await silverTask;

        if (gold != null)
        {
            UpdatePriceDisplay(LondonGold, gold.Last, gold.Chg, gold.ChgPct);
            if (_previousGoldPrice == 0) _previousGoldPrice = gold.Last;

            // 计算人民币金价
            var cnyPrice = _apiService.ConvertToCnyPerGram(gold.Last);
            var cnyChange = _apiService.CalculateCnyChange(gold.Last, _previousGoldPrice);
            var cnyChangePct = _apiService.CalculateCnyChangePercent(gold.Last, _previousGoldPrice);
            UpdatePriceDisplay(CnyGold, cnyPrice, cnyChange, cnyChangePct);

            _previousGoldPrice = gold.Last;

            // 添加到历史记录
            var pricePoint = new PricePoint
            {
                Time = DateTime.Now,
                Price = gold.Last
            };
            RunOnUiThread(() =>
            {
                PriceHistory.Add(pricePoint);

                var maxPoints = GetMaxHistoryPoints();
                while (PriceHistory.Count > maxPoints)
                {
                    PriceHistory.RemoveAt(0);
                }
            });
        }

        if (silver != null)
        {
            UpdatePriceDisplay(LondonSilver, silver.Last, silver.Chg, silver.ChgPct);
        }

        LastUpdateTime = DateTime.Now.ToString("HH:mm:ss");
        StatusMessage = "数据已更新";
        IsLoading = false;
    }

    private void UpdatePriceDisplay(DisplayPrice display, decimal price, decimal change, decimal changePercent)
    {
        display.Price = price;
        display.Change = change;
        display.ChangePercent = changePercent;
        display.IsPositive = change >= 0;
        display.FormattedPrice = price.ToString("N2");
        display.FormattedChange = $"{(display.IsPositive ? "🔼" : "🔽")}{Math.Abs(changePercent):F2}%";

        OnPropertyChanged(nameof(LondonGold));
        OnPropertyChanged(nameof(LondonSilver));
        OnPropertyChanged(nameof(CnyGold));
    }

    public void Stop()
    {
        _cts.Cancel();
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    private static void RunOnUiThread(Action action)
    {
        var dispatcher = System.Windows.Application.Current?.Dispatcher;
        if (dispatcher == null || dispatcher.CheckAccess())
        {
            action();
            return;
        }

        dispatcher.Invoke(action);
    }

    private int GetMaxHistoryPoints()
    {
        var intervalMs = _settings.RefreshIntervalMs <= 0 ? 12000 : _settings.RefreshIntervalMs;
        var max = (int)(TimeSpan.FromDays(7).TotalMilliseconds / intervalMs) + 1;
        return Math.Max(100, max);
    }
}
