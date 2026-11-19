using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Net;
using System.Net.Http;
using UltimaMilla.Repartidor.Models;
using UltimaMilla.Repartidor.Services;

namespace UltimaMilla.Repartidor.ViewModels;

[QueryProperty(nameof(ParadaId), "ParadaId")]
public partial class ParadaDetalleViewModel : ObservableObject
{
    private readonly ApiParadasService _api;

    [ObservableProperty] private int _paradaId;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(MarcarEntregadaCommand))]
    [NotifyCanExecuteChangedFor(nameof(MarcarFallidaCommand))]
    [NotifyCanExecuteChangedFor(nameof(ReintentarEntregaCommand))]
    private Parada? _parada;

    [ObservableProperty] private string _mensajeEstado = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(MarcarFallidaCommand))]
    private string _razonFallo = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(MarcarEntregadaCommand))]
    [NotifyCanExecuteChangedFor(nameof(MarcarFallidaCommand))]
    [NotifyCanExecuteChangedFor(nameof(ReintentarEntregaCommand))]
    private bool _estaGuardando;

    public ParadaDetalleViewModel(ApiParadasService api)
    {
        _api = api;
    }

    partial void OnParadaIdChanged(int value)
    {
        _ = CargarAsync();
    }

    private async Task CargarAsync()
    {
        try
        {
            MensajeEstado = string.Empty;
            Parada = await _api.ObtenerParadaAsync(ParadaId);

            if (Parada is null)
                MensajeEstado = "Esa parada ya no existe en el servidor.";
        }
        catch (HttpRequestException ex)
        {
            MensajeEstado = FormatearErrorHttp(ex, "al cargar la parada");
        }
        catch (TaskCanceledException)
        {
            MensajeEstado = "Tiempo de espera agotado al cargar la parada.";
        }
        catch (Exception ex)
        {
            MensajeEstado = $"Error al cargar la parada: {ex.Message}";
        }
    }

    [RelayCommand(CanExecute = nameof(PuedeMarcarEntregada))]
    public async Task MarcarEntregadaAsync()
    {
        if (Parada is null || !PuedeMarcarEntregada())
            return;

        EstaGuardando = true;
        MensajeEstado = string.Empty;

        var parada = Parada;
        var estadoAnterior = parada.Estado;
        var razonAnterior = parada.RazonFallo;

        try
        {
            parada.Estado = EstadoEntrega.Entregada;
            parada.RazonFallo = null;

            Parada = await _api.ActualizarParadaAsync(parada);
            await Shell.Current.GoToAsync("..");
        }
        catch (HttpRequestException ex)
        {
            parada.Estado = estadoAnterior;
            parada.RazonFallo = razonAnterior;
            MensajeEstado = FormatearErrorHttp(ex, "al guardar");
        }
        catch (TaskCanceledException)
        {
            parada.Estado = estadoAnterior;
            parada.RazonFallo = razonAnterior;
            MensajeEstado = "No se pudo guardar: tiempo de espera agotado.";
        }
        catch (Exception ex)
        {
            parada.Estado = estadoAnterior;
            parada.RazonFallo = razonAnterior;
            MensajeEstado = $"Error al marcar la parada como entregada: {ex.Message}";
        }
        finally
        {
            OnPropertyChanged(nameof(Parada));
            EstaGuardando = false;
        }
    }

    [RelayCommand(CanExecute = nameof(PuedeMarcarFallida))]
    private async Task MarcarFallidaAsync()
    {
        if (Parada is null || !PuedeMarcarFallida())
            return;

        EstaGuardando = true;
        MensajeEstado = string.Empty;

        var parada = Parada;
        var estadoAnterior = parada.Estado;
        var razonAnterior = parada.RazonFallo;

        try
        {
            parada.Estado = EstadoEntrega.Fallida;
            parada.RazonFallo = RazonFallo.Trim();

            Parada = await _api.ActualizarParadaAsync(parada);
            await Shell.Current.GoToAsync("..");
        }
        catch (HttpRequestException ex)
        {
            parada.Estado = estadoAnterior;
            parada.RazonFallo = razonAnterior;
            MensajeEstado = FormatearErrorHttp(ex, "al guardar");
        }
        catch (TaskCanceledException)
        {
            parada.Estado = estadoAnterior;
            parada.RazonFallo = razonAnterior;
            MensajeEstado = "No se pudo guardar: tiempo de espera agotado.";
        }
        catch (Exception ex)
        {
            parada.Estado = estadoAnterior;
            parada.RazonFallo = razonAnterior;
            MensajeEstado = $"Error al marcar la parada como fallida: {ex.Message}";
        }
        finally
        {
            OnPropertyChanged(nameof(Parada));
            EstaGuardando = false;
        }
    }

    [RelayCommand(CanExecute = nameof(PuedeReintentarEntrega))]
    private async Task ReintentarEntregaAsync()
    {
        if (Parada is null || !PuedeReintentarEntrega())
            return;

        EstaGuardando = true;
        MensajeEstado = string.Empty;

        var parada = Parada;
        var estadoAnterior = parada.Estado;
        var razonAnterior = parada.RazonFallo;

        try
        {
            parada.Estado = EstadoEntrega.Pendiente;
            parada.RazonFallo = null;

            Parada = await _api.ActualizarParadaAsync(parada);
            RazonFallo = string.Empty;
            await Shell.Current.GoToAsync("..");
        }
        catch (HttpRequestException ex)
        {
            parada.Estado = estadoAnterior;
            parada.RazonFallo = razonAnterior;
            MensajeEstado = FormatearErrorHttp(ex, "al guardar");
        }
        catch (TaskCanceledException)
        {
            parada.Estado = estadoAnterior;
            parada.RazonFallo = razonAnterior;
            MensajeEstado = "No se pudo guardar: tiempo de espera agotado.";
        }
        catch (Exception ex)
        {
            parada.Estado = estadoAnterior;
            parada.RazonFallo = razonAnterior;
            MensajeEstado = $"Error al reintentar la entrega: {ex.Message}";
        }
        finally
        {
            OnPropertyChanged(nameof(Parada));
            EstaGuardando = false;
        }
    }

    private static string FormatearErrorHttp(HttpRequestException ex, string contexto)
    {
        return ex.StatusCode switch
        {
            null => $"Sin conexión con el servidor {contexto}: {ex.Message}",
            HttpStatusCode.NotFound => $"La parada ya no existe en el servidor (404) {contexto}.",
            HttpStatusCode.InternalServerError => $"Error interno del servidor (500) {contexto}. Intenta más tarde.",
            _ => $"Error HTTP ({(int)ex.StatusCode}) {contexto}: {ex.Message}"
        };
    }

    private bool PuedeMarcarEntregada()
    {
        return Parada is not null &&
               ReglasEntrega.PuedeEntregar(Parada) &&
               !EstaGuardando;
    }

    private bool PuedeMarcarFallida()
    {
        return Parada is not null &&
               ReglasEntrega.PuedeFallar(Parada) &&
               !string.IsNullOrWhiteSpace(RazonFallo) &&
               RazonFallo.Trim().Length >= 10 &&
               !EstaGuardando;
    }

    private bool PuedeReintentarEntrega()
    {
        return Parada is not null &&
               ReglasEntrega.PuedeReintentar(Parada) &&
               !EstaGuardando;
    }
}