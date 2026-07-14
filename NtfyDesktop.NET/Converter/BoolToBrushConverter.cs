using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace NtfyDesktop.NET.Converter;

public class BoolToBrushConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is true)
            return new SolidColorBrush(Color.Parse("#4CAF50")); // Material Green
        return new SolidColorBrush(Color.Parse("#d43434")); // Use default foreground
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
