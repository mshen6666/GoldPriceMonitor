using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace GoldPriceMonitor.Utils;

public class BooleanToBrushConverter : IValueConverter
{
    public SolidColorBrush TrueBrush { get; set; } = new(Colors.Green);
    public SolidColorBrush FalseBrush { get; set; } = new(Colors.Red);

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return (value is true) ? TrueBrush : FalseBrush;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class EnumToBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null || parameter == null) return false;
        return value.ToString() == parameter.ToString();
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is true && parameter != null)
        {
            return Enum.Parse(targetType, parameter.ToString()!);
        }
        return Enum.Parse(targetType, "0");
    }
}
