using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Net;
using System.Net.Http;
using UltimaMilla.Repartidor.Models;
using UltimaMilla.Repartidor.Services;

namespace UltimaMilla.Repartidor.ViewModels;

public partial class HistoricoViewModel : ObservableObject
{
    private readonly ApiParadasService _api;
    private readonly SesionActual _sesionActual;

    [ObservableProperty] private int _entregasEstaSemana;
    [ObservableProperty] private int _entregasEsteMes;
    [ObservableProperty] private bool _estaCargando;
    [ObservableProperty] private string _mensajeEstado = string.Empty;

    public ObservableCollection<Parada> EntregasDelMes { get; } = new();

    public HistoricoViewModel(ApiParadasService api, SesionActual sesionActual)
    {
        _api = api;
        _sesionActual = sesionActual;
    }

    [RelayCommand]
    private async Task CargarHistoricoMesAsync()
    {
        if (_sesionActual.RepartidorId == 0)
        {
            EntregasDelMes.Clear();
            EntregasEstaSemana = 0;
            EntregasEsteMes = 0;
            await Shell.Current.GoToAsync("//LoginPage");
            return;
        }

        try
        {
            EstaCargando = true;
            MensajeEstado = string.Empty;

            var todas = await _api.ObtenerParadasAsync(_sesionActual.RepartidorId);
            var ahora = DateTime.UtcNow;
            var desdeSemana = ahora.AddDays(-7);
            var desdeMes = new DateTime(ahora.Year, ahora.Month, 1, 0, 0, 0, DateTimeKind.Utc);

            EntregasEstaSemana = todas.Count(p => p.Estado == EstadoEntrega.Entregada && p.ModificadoEn >= desdeSemana);

            var delMes = todas
                .Where(p => p.Estado == EstadoEntrega.Entregada && p.ModificadoEn >= desdeMes)
                .OrderByDescending(p => p.ModificadoEn)
                .ToList();

            EntregasEsteMes = delMes.Count;

            EntregasDelMes.Clear();
            foreach (var p in delMes) EntregasDelMes.Add(p);
        }
        catch (HttpRequestException ex)
        {
            MensajeEstado = ex.StatusCode switch
            {
                null => $"Sin conexión con el servidor al cargar el histórico: {ex.Message}",
                HttpStatusCode.NotFound => "No se encontró el recurso en el servidor (404).",
                HttpStatusCode.InternalServerError => "Error interno del servidor (500). Intenta más tarde.",
                _ => $"Error HTTP ({(int)ex.StatusCode}) al cargar el histórico: {ex.Message}"
            };
        }
        catch (TaskCanceledException)
        {
            MensajeEstado = "Tiempo de espera agotado al cargar el histórico.";
        }
        catch (Exception ex)
        {
            MensajeEstado = $"Error al cargar el histórico: {ex.Message}";
        }
        finally
        {
            EstaCargando = false;
        }
    }
}