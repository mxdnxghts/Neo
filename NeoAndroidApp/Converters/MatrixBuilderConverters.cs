using System.Globalization;

namespace NeoAndroidApp.Converters;

/// <summary>
/// Converts a string to a boolean indicating whether it's a valid double value.
/// Used for validation highlighting in the matrix builder.
/// </summary>
public class IsValidDoubleConverter : IValueConverter
{
    /// <summary>
    /// Converts a string to true if it's a valid double, false otherwise.
    /// </summary>
    /// <param name="value">The string value to validate.</param>
    /// <param name="targetType">The target type (unused).</param>
    /// <param name="parameter">Converter parameter (unused).</param>
    /// <param name="culture">The culture (unused, uses InvariantCulture).</param>
    /// <returns>True if the string is a valid double; otherwise, false.</returns>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string str)
        {
            return double.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out _);
        }

        return false;
    }

    /// <summary>
    /// Converts back (not supported).
    /// </summary>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}

/// <summary>
/// Inverts a boolean value. Used for enabling/disabling UI elements based on opposite conditions.
/// </summary>
public class InverseBoolConverter : IValueConverter
{
    /// <summary>
    /// Inverts the boolean value.
    /// </summary>
    /// <param name="value">The boolean value to invert.</param>
    /// <param name="targetType">The target type (unused).</param>
    /// <param name="parameter">Converter parameter (unused).</param>
    /// <param name="culture">The culture (unused).</param>
    /// <returns>The inverted boolean value.</returns>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
            return !boolValue;

        return true;
    }

    /// <summary>
    /// Converts back by inverting again.
    /// </summary>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
            return !boolValue;

        return true;
    }
}
