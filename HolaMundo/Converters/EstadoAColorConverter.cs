using System.Globalization;
using UltimaMilla.Repartidor.Models;

namespace UltimaMilla.Repartidor.Converters;

public class EstadoAColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        bool oscuro = Application.Current?.RequestedTheme == AppTheme.Dark;

        return value switch
        {
            EstadoEntrega.Entregada => oscuro ? Color.FromArgb("#66BB6A") : Color.FromArgb("#2E7D32"),
            EstadoEntrega.Fallida => oscuro ? Color.FromArgb("#EF5350") : Color.FromArgb("#C62828"),
            _ => oscuro ? Color.FromArgb("#8C9EFF") : Color.FromArgb("#3949AB"), // Pendiente
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}