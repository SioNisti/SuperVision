using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SuperVision
{
    public class VarTypeToBooleanConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string currentType && parameter is string targetTypeStr)
            {
                return currentType == targetTypeStr;
            }
            return false;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => null;
    }

    public class StringBoolConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return bool.TryParse(value?.ToString(), out bool result) && result;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return (value is bool b && b) ? "True" : "False";
        }
    }

    public class ColorJsonConverter : JsonConverter<Color>
    {
        public override Color Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string? hex = reader.GetString();
            if (Color.TryParse(hex, out Color color))
            {
                return color;
            }
            return Colors.White; // Fallback if parsing fails
        }

        public override void Write(Utf8JsonWriter writer, Color value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
    public class ColorToBrushConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is Color color)
            {
                return new SolidColorBrush(color);
            }
            return Brushes.White;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is SolidColorBrush brush)
            {
                return brush.Color;
            }
            return Colors.White;
        }
    }
}