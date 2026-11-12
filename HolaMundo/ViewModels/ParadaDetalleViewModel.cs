using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UltimaMilla.Repartidor.Models;
using UltimaMilla.Repartidor.Services;

namespace UltimaMilla.Repartidor.ViewModels;

[QueryProperty(nameof(ParadaId), "ParadaId")]
public partial class ParadaDetalleViewModel : ObservableObject
{
    private readonly ParadasDatabase _paradasDatabase;

    [ObservableProperty] private int _paradaId;

    [ObservableProperty]
    // Se eliminó NotifyPropertyChangedFor(PuedeEditar) porque ya delegamos en ReglasEntrega
    [NotifyCanExecuteChangedFor(nameof(MarcarEntregadaCommand))]
    [NotifyCanExecuteChangedFor(nameof(MarcarFallidaCommand))]
    [NotifyCanExecuteChangedFor(nameof(ReintentarEntregaCommand))]
    private Parada? _parada;

    [ObservableProperty] private string _mensajeEstado = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(MarcarFallidaCommand))]
    private string _razonFallo = string.Empty;

    [ObservableProperty]
    // Se eliminó NotifyPropertyChangedFor(PuedeEditar)
    [NotifyCanExecuteChangedFor(nameof(MarcarEntregadaCommand))]
    [NotifyCanExecuteChangedFor(nameof(MarcarFallidaCommand))]
    [NotifyCanExecuteChangedFor(nameof(ReintentarEntregaCommand))]
    private bool _estaGuardando;

    public ParadaDetalleViewModel(ParadasDatabase paradasDatabase)
    {
        _paradasDatabase = paradasDatabase;
    }

    partial void OnParadaIdChanged(int value)
    {
        _ = CargarAsync();
    }

    private async Task CargarAsync()
    {
        Parada = await _paradasDatabase.GetParadaAsync(new Parada { Id = ParadaId });
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
        var fechaAnterior = parada.ModificadoEn;

        try
        {
            parada.Estado = EstadoEntrega.Entregada; //EJERCICIO 7
            parada.RazonFallo = null;
            parada.ModificadoEn = DateTime.Now;

            await _paradasDatabase.SaveParadaAsync(parada);
            MensajeEstado = "Parada marcada como entregada.";
        }
        catch (Exception ex)
        {
            parada.Estado = estadoAnterior;
            parada.RazonFallo = razonAnterior;
            parada.ModificadoEn = fechaAnterior;

            MensajeEstado = $"Error al marcar la parada como entregada: {ex.Message}";
            return;
        }
        finally
        {
            OnPropertyChanged(nameof(Parada));
            EstaGuardando = false;
        }

        await Shell.Current.GoToAsync("..");
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
        var fechaAnterior = parada.ModificadoEn;

        try
        {
            parada.Estado = EstadoEntrega.Fallida; //EJERCICIO 7
            parada.RazonFallo = RazonFallo.Trim();
            parada.ModificadoEn = DateTime.Now;

            await _paradasDatabase.SaveParadaAsync(parada);
            MensajeEstado = "Parada marcada como fallida.";
        }
        catch (Exception ex)
        {
            parada.Estado = estadoAnterior;
            parada.RazonFallo = razonAnterior;
            parada.ModificadoEn = fechaAnterior;

            MensajeEstado = $"Error al marcar la parada como fallida: {ex.Message}";
            return;
        }
        finally
        {
            OnPropertyChanged(nameof(Parada));
            EstaGuardando = false;
        }

        await Shell.Current.GoToAsync("..");
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
        var fechaAnterior = parada.ModificadoEn;

        try
        {
            parada.Estado = EstadoEntrega.Pendiente; //EJERCICIO 7
            parada.RazonFallo = null;
            parada.ModificadoEn = DateTime.Now;

            await Task.Delay(5000); //Solo para probar por 5 segundos
            await _paradasDatabase.SaveParadaAsync(parada);

            RazonFallo = string.Empty;
            MensajeEstado = "Entrega lista para reintentarse.";
        }
        catch (Exception ex)
        {
            parada.Estado = estadoAnterior;
            parada.RazonFallo = razonAnterior;
            parada.ModificadoEn = fechaAnterior;

            MensajeEstado = $"Error al reintentar la entrega: {ex.Message}";
            return;
        }
        finally
        {
            OnPropertyChanged(nameof(Parada));
            EstaGuardando = false;
        }

        await Shell.Current.GoToAsync("..");
    }

    //EJERCICIO 7
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
               RazonFallo.Trim().Length >= 10 && // RETO N5, SE AGREGA UNA REGLA AL ESCRIBIR Y CON TRIM VALIDA QUE SI SE ESCRIBEN ESPACIOS VACIOS NO VALGAN
               !EstaGuardando;
    }

    private bool PuedeReintentarEntrega()
    {
        return Parada is not null &&
               ReglasEntrega.PuedeReintentar(Parada) &&
               !EstaGuardando;
    }
}
