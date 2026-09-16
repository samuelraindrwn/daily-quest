using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace DailyQuest.Infrastructure;

public sealed class HexColorBrushConverter : IValueConverter
{
    private static readonly SolidColorBrush FallbackBrush = CreateFrozenBrush(
        Color.FromRgb(75, 143, 119));
    private static readonly SolidColorBrush LightForeground = CreateFrozenBrush(Colors.White);
    private static readonly SolidColorBrush DarkForeground = CreateFrozenBrush(
        Color.FromRgb(23, 31, 43));

    public object Convert(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture)
    {
        var color = TryParseColor(value as string, out var parsed)
            ? parsed
            : FallbackBrush.Color;

        if (string.Equals(parameter as string, "foreground", StringComparison.OrdinalIgnoreCase))
        {
            // Relative luminance keeps user-selected bright label colors readable.
            var luminance = ((0.2126 * color.R) + (0.7152 * color.G) + (0.0722 * color.B)) / 255;
            return luminance > 0.63 ? DarkForeground : LightForeground;
        }

        return CreateFrozenBrush(color);
    }

    public object ConvertBack(
        object value,
        Type targetType,
        object parameter,
        CultureInfo culture) => Binding.DoNothing;

    private static bool TryParseColor(string? value, out Color color)
    {
        color = default;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var normalized = value.Trim();
        if (!normalized.StartsWith('#'))
        {
            normalized = $"#{normalized}";
        }

        try
        {
            var converted = ColorConverter.ConvertFromString(normalized);
            if (converted is Color parsed && normalized.Length is 7 or 9)
            {
                color = parsed;
                return true;
            }
        }
        catch (Exception exception) when (exception is FormatException or NotSupportedException)
        {
            // Invalid user input falls back to the app accent until it is corrected.
        }

        return false;
    }

    private static SolidColorBrush CreateFrozenBrush(Color color)
    {
        var brush = new SolidColorBrush(color);
        brush.Freeze();
        return brush;
    }
}
