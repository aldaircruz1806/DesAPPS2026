using System.Globalization;
using UltimaMilla.Repartidor.Helpers;

namespace UltimaMilla.Repartidor.Converters;

public class UtcALocalConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is DateTime dt ? dt.AHoraLocal() : value;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}