using System.Globalization;
using UltimaMilla.Repartidor.Models;

namespace UltimaMilla.Repartidor.Converters;

public class EstadoADisplayConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value switch
        {
            EstadoEntrega.Entregada => "✅ Entregada",
            EstadoEntrega.Fallida => "❌ Fallida",
            _ => "⏳ Pendiente",
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}