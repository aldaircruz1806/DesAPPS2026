using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.ApplicationModel;

namespace UltimaMilla.Repartidor.Services;

public class UbicacionService
{
    public async Task<Location?> ObtenerUbicacionActualAsync()
    {
        try
        {
            // Obligamos al sistema a pedir el permiso en el hilo de la interfaz
            var estado = await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                return await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            });

            if (estado != PermissionStatus.Granted)
                return null;

            var solicitud = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));
            return await Geolocation.Default.GetLocationAsync(solicitud);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"\n 🚨 ERROR DE GPS: {ex.Message} \n");
            return null;
        }
    }
}