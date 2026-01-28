using Avalonia.Data.Converters;
using System;
using System.Globalization;

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
}