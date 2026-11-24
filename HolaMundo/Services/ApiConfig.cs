namespace UltimaMilla.Repartidor.Services;

public static class ApiConfig
{
    // true = tu API en Render (Nube) · false = tu API local
    private const bool UsarNube = true;

    // IMPORTANTE: Asegúrate de que termine con una diagonal /
    private const string UrlNube = "https://ultimamilla-api-lopez.onrender.com/";

    public static string BaseUrl => UsarNube
        ? UrlNube
        : DeviceInfo.Platform == DevicePlatform.Android
            ? "http://10.0.2.2:5199/"      // emulador Android -> tu PC
            : "http://localhost:5199/";    // Windows / simulador iOS
}