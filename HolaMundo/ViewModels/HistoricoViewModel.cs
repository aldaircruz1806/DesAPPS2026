using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using UltimaMilla.Repartidor.Models;
using UltimaMilla.Repartidor.Services;

namespace UltimaMilla.Repartidor.ViewModels;

public partial class HistoricoViewModel : ObservableObject
{
    private readonly ParadasDatabase _paradasDatabase;

    [ObservableProperty] private int _entregasEstaSemana;
    [ObservableProperty] private int _entregasEsteMes;
    [ObservableProperty] private bool _estaCargando;
    [ObservableProperty] private string _mensajeEstado = string.Empty;

    public ObservableCollection<Parada> EntregasDelMes { get; } = new();

    public HistoricoViewModel(ParadasDatabase paradasDatabase)
    {
        _paradasDatabase = paradasDatabase;
    }

    [RelayCommand]
    private async Task CargarHistoricoMesAsync()
    {
        try
        {
            EstaCargando = true;
            MensajeEstado = string.Empty;

            var todas = await _paradasDatabase.GetParadasAsync(true); // ya viene filtrado por tu RepartidorId
            var ahora = DateTime.Now;
            var desdeSemana = ahora.AddDays(-7);
            var desdeMes = new DateTime(ahora.Year, ahora.Month, 1);

            EntregasEstaSemana = todas.Count(p => p.Estado == "Entregado" && p.ModificadoEn >= desdeSemana);

            var delMes = todas
                .Where(p => p.Estado == "Entregado" && p.ModificadoEn >= desdeMes)
                .OrderByDescending(p => p.ModificadoEn)
                .ToList();

            EntregasEsteMes = delMes.Count;

            EntregasDelMes.Clear();
            foreach (var p in delMes) EntregasDelMes.Add(p);
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
