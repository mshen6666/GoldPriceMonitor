using GoldPriceMonitor.Models;
using GoldPriceMonitor.Services;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace GoldPriceMonitor.ViewModels;

public class ChartViewModel : INotifyPropertyChanged
{
    public enum TimeRange { OneHour, SixHours, TwentyFourHours, SevenDays }

    private readonly GoldApiService _apiService;
    private TimeRange _selectedRange = TimeRange.OneHour;
    private readonly ObservableCollection<DateTimePoint> _filteredHistory;

    public ISeries[] Series { get; private set; } = Array.Empty<ISeries>();
    public Axis[] XAxes { get; private set; } = Array.Empty<Axis>();
    public Axis[] YAxes { get; private set; } = Array.Empty<Axis>();

    public TimeRange SelectedRange
    {
        get => _selectedRange;
        set
        {
            if (SetProperty(ref _selectedRange, value))
            {
                UpdateChart();
            }
        }
    }

    public ObservableCollection<DateTimePoint> FilteredHistory => _filteredHistory;

    public ChartViewModel(GoldApiService apiService, ObservableCollection<PricePoint> history)
    {
        _apiService = apiService;
        _filteredHistory = new ObservableCollection<DateTimePoint>();

        XAxes = new Axis[]
        {
            new Axis
            {
                Labeler = value => new DateTime((long)value).ToString("HH:mm"),
                TextSize = 12,
                LabelsRotation = 0,
                UnitWidth = TimeSpan.FromMinutes(1).Ticks
            }
        };

        YAxes = new Axis[]
        {
            new Axis
            {
                TextSize = 12,
                SeparatorsPaint = new SolidColorPaint(SKColors.LightGray)
            }
        };

        UpdateFromHistory(history);
    }

    public void UpdateFromHistory(ObservableCollection<PricePoint> history)
    {
        _filteredHistory.Clear();
        var cutoffTime = GetCutoffTime();

        foreach (var point in history.Where(p => p.Time >= cutoffTime))
        {
            _filteredHistory.Add(new DateTimePoint(point.Time, (double)point.Price));
        }

        UpdateChart();
    }

    private DateTime GetCutoffTime()
    {
        return SelectedRange switch
        {
            TimeRange.OneHour => DateTime.Now.AddHours(-1),
            TimeRange.SixHours => DateTime.Now.AddHours(-6),
            TimeRange.TwentyFourHours => DateTime.Now.AddHours(-24),
            TimeRange.SevenDays => DateTime.Now.AddDays(-7),
            _ => DateTime.Now.AddHours(-1)
        };
    }

    private void UpdateChart()
    {
        Series = new ISeries[]
        {
            new LineSeries<DateTimePoint>
            {
                Values = _filteredHistory,
                Stroke = new SolidColorPaint(SKColors.Gold, 2),
                Fill = new SolidColorPaint(new SKColor(255, 215, 0, 40)),
                GeometrySize = 6,
                GeometryStroke = new SolidColorPaint(SKColors.Gold, 2),
                GeometryFill = new SolidColorPaint(SKColors.Gold),
                LineSmoothness = 0.3
            }
        };

        OnPropertyChanged(nameof(Series));
    }

    public event PropertyChangedEventHandler? PropertyChanged;

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
}
