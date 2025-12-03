using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace MatchQuest.App.Converters;

public class EqualityToBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (parameter is null)
            return false;

        var paramStr = parameter.ToString() ?? string.Empty;
        var negate = false;
        if (paramStr.StartsWith("!"))
        {
            negate = true;
            paramStr = paramStr.Substring(1);
        }

        // Try numeric comparison first
        if (int.TryParse(paramStr, out var paramInt))
        {
            try
            {
                var valueInt = System.Convert.ToInt32(value, culture);
                var equals = valueInt == paramInt;
                return negate ? !equals : equals;
            }
            catch
            {
                // fall through to string compare
            }
        }

        var equalsStr = string.Equals(value?.ToString(), paramStr, StringComparison.OrdinalIgnoreCase);
        return negate ? !equalsStr : equalsStr;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}