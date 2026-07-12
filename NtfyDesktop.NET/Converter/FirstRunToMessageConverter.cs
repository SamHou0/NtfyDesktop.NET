using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace NtfyDesktop.NET.Converter;

public class FirstRunToMessageConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        bool isFirstRun = (bool)(value ?? throw new ArgumentNullException(nameof(value)));
        return isFirstRun ? "Tips: Add a topic and choose it first!" : "Welcome back!";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}