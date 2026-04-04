using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;

namespace FastTelecom.AvaloniaUI.Converters
{
    public sealed class PercentToBrushConverter : IValueConverter
    {
        public static readonly PercentToBrushConverter Instance = new();

        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var pct = value is double d ? d : 0;
            return pct switch
            {
                < 60 => new SolidColorBrush(Color.Parse("#16A34A")), 
                < 80 => new SolidColorBrush(Color.Parse("#D97706")),
                _    => new SolidColorBrush(Color.Parse("#DC2626")), 
            };
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
