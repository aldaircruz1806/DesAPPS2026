using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Net;
using System.Net.Http;
using UltimaMilla.Repartidor.Models;
using UltimaMilla.Repartidor.Services;

namespace UltimaMilla.Repartidor.ViewModels;

public partial class JornadaViewModel : ObservableObject
{
    private readonly ApiParadasService _api;
    private readonly ApiTurnosService _apiTurnos;
    private readonly RepartidorDatabase _repartidorDatabase;
    private readonly SesionActual _sesionActual;
    private readonly UbicacionService _ubicacionService;

    // Temporizador para el tiempo activo del turno (Reto 6.2)
    private readonly IDispatcherTimer _timer;

    // Temporizador para actualizar ubicación y paradas (Reto 6.5)
    private readonly IDispatcherTimer _timerGps;

    [ObservableProperty] private string _saludo = string.Empty;
    [ObservableProperty] private int _totalEntregas = 0;
    [ObservableProperty] private int _entregasCompletadas = 0;
    [ObservableProperty] private int _entregasPendientes = 0;
    [ObservableProperty] private bool _isRefrescando = false;
    [ObservableProperty] private bool _sinFiltro = false;
    [ObservableProperty] private Parada? _paradaSeleccionada;
    [ObservableProperty] private string _tiempoTranscurrido = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TextoBanner))]
    [NotifyPropertyChangedFor(nameof(HayBanner))]
    private string _mensajeEstado = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TextoBanner))]
    [NotifyPropertyChangedFor(nameof(HayBanner))]
    [NotifyPropertyChangedFor(nameof(HayError))]
    private string _mensajeError = string.Empty;

    // Propiedades calculadas para el banner de estado/error
    public bool HayError => !string.IsNullOrWhiteSpace(MensajeError);
    public string TextoBanner => HayError ? MensajeError : MensajeEstado;
    public bool HayBanner => !string.IsNullOrWhiteSpace(TextoBanner);

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TextoBotonTurno))]
    private Turno? turnoActivo;

    public ObservableCollection<Parada> Paradas { get; } = new();

    public JornadaViewModel(
        ApiParadasService api,
        ApiTurnosService apiTurnos,
        RepartidorDatabase repartidorDatabase,
        SesionActual sesionActual,
        UbicacionService ubicacionService)
    {
        _api = api;
        _apiTurnos = apiTurnos;
        _repartidorDatabase = repartidorDatabase;
        _sesionActual = sesionActual;
        _ubicacionService = ubicacionService;

        _timer = Application.Current!.Dispatcher.CreateTimer();
        _timer.Interval = TimeSpan.FromSeconds(1);
        _timer.Tick += (s, e) => CalcularTiempo();

        _timerGps = Application.Current.Dispatcher.CreateTimer();
        _timerGps.Interval = TimeSpan.FromSeconds(30);
        _timerGps.Tick += (s, e) =>
        {
            if (!IsRefrescando)
            {
                _ = RefrescaParadas();
            }
        };
        _timerGps.Start();
    }

    [RelayCommand]
    private async Task RefrescaParadas()
    {
        if (IsRefrescando)
            return;

        if (_sesionActual.RepartidorId == 0)
        {
            Paradas.Clear();
            TotalEntregas = 0;
            EntregasCompletadas = 0;
            EntregasPendientes = 0;
            Saludo = string.Empty;

            await Shell.Current.GoToAsync("//LoginPage");
            return;
        }

        try
        {
            IsRefrescando = true;
            MensajeError = string.Empty;
            MensajeEstado = string.Empty;

            var repartidor = await _repartidorDatabase.GetRepartidorAsync(_sesionActual.RepartidorId);

            if (repartidor is null)
            {
                Preferences.Default.Remove("RepartidorActualId");
                _sesionActual.RepartidorId = 0;

                Paradas.Clear();
                TotalEntregas = 0;
                EntregasCompletadas = 0;
                EntregasPendientes = 0;
                Saludo = string.Empty;

                await Shell.Current.GoToAsync("//LoginPage");
                return;
            }

            FormarSaludo(repartidor.Nombre);

            // Restaura el turno abierto desde el servidor
            TurnoActivo = await _apiTurnos.GetTurnoAbiertoAsync();
            if (TurnoActivo != null)
            {
                CalcularTiempo();
                _timer.Start();
            }
            else
            {
                _timer.Stop();
                TiempoTranscurrido = string.Empty;
            }

            // Las paradas vienen del servidor (API REST)
            var paradas = await _api.ObtenerParadasAsync(_sesionActual.RepartidorId);

            var miUbicacion = await _ubicacionService.ObtenerUbicacionActualAsync();
            if (miUbicacion == null)
            {
                MensajeEstado = "Activa la ubicación para ver distancias";
            }

            // Cálculo de distancias (Reto 6.5)
            foreach (var p in paradas)
            {
                if (miUbicacion is not null && p.Latitud is not null && p.Longitud is not null)
                {
                    p.DistanciaKm = UltimaMilla.Repartidor.Helpers.Distancia.EnKilometros(
                        miUbicacion.Latitude, miUbicacion.Longitude,
                        p.Latitud.Value, p.Longitud.Value);

                    p.DistanciaTexto = $"{p.DistanciaKm:F1} km";
                }
                else
                {
                    p.DistanciaKm = double.MaxValue;
                    p.DistanciaTexto = "Sin ubicación";
                }
            }

            paradas = paradas.OrderBy(p => p.DistanciaKm).ToList();

            TotalEntregas = paradas.Count;
            EntregasCompletadas = paradas.Count(p => p.Estado == EstadoEntrega.Entregada);
            EntregasPendientes = paradas.Count(p => p.Estado == EstadoEntrega.Pendiente);
            ParadaSeleccionada = null;

            Paradas.Clear();
            foreach (var parada in paradas)
            {
                if (SinFiltro || parada.Estado != EstadoEntrega.Entregada)
                {
                    Paradas.Add(parada);
                }
            }

            if (string.IsNullOrEmpty(MensajeEstado))
            {
                MensajeEstado = paradas.Count == 0
                    ? "El servidor no tiene paradas asignadas."
                    : Paradas.Count == 0
                        ? "No hay paradas pendientes ni fallidas."
                        : "¡Datos actualizados correctamente!";
            }
        }
        catch (HttpRequestException ex)
        {
            MensajeError = ex.StatusCode switch
            {
                null => $"Sin conexión con el servidor: {ex.Message}",
                HttpStatusCode.NotFound => "No se encontró el recurso en el servidor (404).",
                HttpStatusCode.InternalServerError => "Error interno en el servidor (500). Intenta más tarde.",
                _ => $"Error HTTP ({(int)ex.StatusCode}): {ex.Message}"
            };
        }
        catch (TaskCanceledException ex)
        {
            MensajeError = $"Tiempo de espera agotado al consultar datos: {ex.Message}";
        }
        catch (Exception ex)
        {
            MensajeError = $"Error al consultar datos: {ex.Message}";
        }
        finally
        {
            IsRefrescando = false;
        }

        if (!HayError && !string.IsNullOrEmpty(MensajeEstado))
        {
            await Task.Delay(3000);
            MensajeEstado = string.Empty;
        }
    }

    partial void OnSinFiltroChanged(bool value)
    {
        _ = RefrescaParadas();
    }

    private void FormarSaludo(string nombreRepartidor)
    {
        string saludo;
        if (DateTime.Now.Hour < 12) saludo = "Buenos días ";
        else if (DateTime.Now.Hour < 20) saludo = "Buenas tardes ";
        else saludo = "Buenas noches ";

        Saludo = saludo + nombreRepartidor;
    }

    public string TextoBotonTurno => TurnoActivo is null ? "Iniciar turno" : "Terminar turno";

    [RelayCommand]
    private async Task AlternarTurnoAsync()
    {
        try
        {
            IsRefrescando = true;
            MensajeError = string.Empty;

            if (TurnoActivo is null)
            {
                TurnoActivo = await _apiTurnos.AbrirTurnoAsync();
                CalcularTiempo();
                _timer.Start();
                MensajeEstado = "Turno iniciado correctamente";
            }
            else
            {
                await _apiTurnos.CerrarTurnoAsync(TurnoActivo.Id);
                TurnoActivo = null;
                _timer.Stop();
                TiempoTranscurrido = string.Empty;
                MensajeEstado = "Turno cerrado correctamente";
            }
        }
        catch (HttpRequestException ex) { MensajeError = $"Error de red al cambiar turno: {ex.Message}"; }
        catch (TaskCanceledException ex) { MensajeError = $"Tiempo de espera agotado: {ex.Message}"; }
        catch (Exception ex) { MensajeError = $"Error al cambiar turno: {ex.Message}"; }
        finally { IsRefrescando = false; }
    }

    private void CalcularTiempo()
    {
        if (TurnoActivo?.FechaInicio != null)
        {
            var diferencia = DateTime.UtcNow - DateTime.SpecifyKind(TurnoActivo.FechaInicio, DateTimeKind.Utc);
            if (diferencia < TimeSpan.Zero) diferencia = TimeSpan.Zero;
            TiempoTranscurrido = $"Tiempo Activo: {diferencia.Hours}h {diferencia.Minutes}m {diferencia.Seconds}s";
        }
    }
}