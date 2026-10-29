using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UltimaMilla.Repartidor.Models;
using UltimaMilla.Repartidor.Services;

namespace UltimaMilla.Repartidor.ViewModels;

public partial class JornadaViewModel : ObservableObject
{
    private readonly ParadasDatabase _paradasDatabase;
    private readonly RepartidorDatabase _repartidorDatabase;
    private readonly SesionActual _sesionActual;
    private readonly TurnoDatabase _turnoDatabase;
    [ObservableProperty] private string _saludo = string.Empty;

    [ObservableProperty] private int _totalEntregas = 0;

    [ObservableProperty] private int _entregasCompletadas = 0;

    [ObservableProperty] private bool _isRefrescando = false;

    [ObservableProperty] private string _mensajeEstado = string.Empty;

    [ObservableProperty] private bool _sinFiltro = false;
    [ObservableProperty] private Parada? _paradaSeleccionada;
    [ObservableProperty][NotifyPropertyChangedFor(nameof(TextoBotonTurno))]private Turno? turnoActivo;
  

    public ObservableCollection<Parada> Paradas { get; } = new();
    public JornadaViewModel(ParadasDatabase paradasDatabase,
        RepartidorDatabase repartidorDatabase, SesionActual sesionActual, TurnoDatabase turnoDatabase)
    {
        _paradasDatabase = paradasDatabase;
        _repartidorDatabase = repartidorDatabase;
        _sesionActual = sesionActual;
        _turnoDatabase = turnoDatabase;
        // Ya no llamamos FormarSaludo() aquí: necesita el nombre real,
        // y eso solo lo sabemos después de una consulta async.
    }

    [RelayCommand]
    private async Task RefrescaParadas()
    {
        // También protege las llamadas directas desde el interruptor.
        if (IsRefrescando)
            return;

        IsRefrescando = true;
        MensajeEstado = string.Empty;

        try
        {

            if (_sesionActual.RepartidorId == 0)
            {
                Paradas.Clear();
                TotalEntregas = 0;
                EntregasCompletadas = 0;
                Saludo = string.Empty;

                await Shell.Current.GoToAsync("//LoginPage");
                return;
            }

            var repartidor = await _repartidorDatabase.GetRepartidorAsync(
                _sesionActual.RepartidorId);

            if (repartidor is null)
            {
                Preferences.Default.Remove("RepartidorActualId");
                _sesionActual.RepartidorId = 0;

                Paradas.Clear();
                TotalEntregas = 0;
                EntregasCompletadas = 0;
                Saludo = string.Empty;

                await Shell.Current.GoToAsync("//LoginPage");
                return;
            }

            FormarSaludo(repartidor.Nombre);
            //SE AGREGA LA CARGA DEL TURNO ABIERTO
            TurnoActivo = await _turnoDatabase.GetTurnoAbiertoAsync();
#if DEBUG
            await Task.Delay(1000);
#endif

            // Consultar TODAS las paradas del repartidor actual.
            var paradas = await _paradasDatabase.GetParadasAsync(true);

#if DEBUG
            // Sembrar solo si realmente no tiene ninguna parada.
            if (paradas.Count == 0)
            {
                await InventaParadas();
                paradas = await _paradasDatabase.GetParadasAsync(true);
            }
#endif

            // Los contadores no dependen del filtro de pantalla.
            TotalEntregas = paradas.Count;
            EntregasCompletadas =
                paradas.Count(p => p.Estado == "Entregado");

            ParadaSeleccionada = null;
            Paradas.Clear();

            foreach (var parada in paradas)
            {
                // SinFiltro=true: mostrar todas.
                // SinFiltro=false: ocultar entregadas, conservar fallidas.
                if (SinFiltro || parada.Estado != "Entregado")
                {
                    Paradas.Add(parada);
                }
            }

            MensajeEstado = paradas.Count == 0
                ? "No hay paradas disponibles."
                : Paradas.Count == 0
                    ? "No hay paradas pendientes ni fallidas."
                    : "¡Datos actualizados correctamente!";

            await Task.Delay(2500);
            MensajeEstado = string.Empty;
        }
        catch (Exception ex)
        {
            MensajeEstado = $"Error al actualizar datos: {ex.Message}";
        }
        finally
        {
            IsRefrescando = false;
        }
    }



    partial void OnSinFiltroChanged(bool value)
    {
        _ = RefrescaParadas();
    }
    private async Task InventaParadas()
    {
        
        await _paradasDatabase.SaveParadaAsync(new Parada { Numero = 1, RepartidorId = _sesionActual.RepartidorId, Cliente = "Farmacia San Juan", Direccion = "Av. Hidalgo 123", Estado = "Pendiente",});
        await _paradasDatabase.SaveParadaAsync(new Parada { Numero = 2, RepartidorId = _sesionActual.RepartidorId, Cliente = "Abarrotes La Esquina", Direccion = "Calle Morelos 45", Estado = "Pendiente",});
        await _paradasDatabase.SaveParadaAsync(new Parada { Numero = 3, RepartidorId = _sesionActual.RepartidorId, Cliente = "Panadería El Buen Pan", Direccion = "Calle Juárez 67", Estado = "Pendiente", });
        await _paradasDatabase.SaveParadaAsync(new Parada { Numero = 4, RepartidorId = _sesionActual.RepartidorId, Cliente = "Don Panchito Perez", Direccion = "Calle Juárez 520", Estado = "Entregado", });
        await _paradasDatabase.SaveParadaAsync(new Parada { Numero = 5, RepartidorId = _sesionActual.RepartidorId, Cliente = "Eulalio de la Cruz", Direccion = "Colon 888, Jalisco", Estado = "Entregado", });
    }
    private void FormarSaludo(string nombreRepartidor)
    {
        string saludo;
        if (DateTime.Now.Hour < 12) saludo = "Buenos días ";
        else if (DateTime.Now.Hour < 20) saludo = "Buenas tardes ";
        else saludo = "Buenas noches ";

        Saludo = saludo + nombreRepartidor;
    }

    //parte 6.2
    public string TextoBotonTurno => TurnoActivo is null ? "Iniciar turno" : "Terminar turno";

    [RelayCommand]
    private async Task AlternarTurnoAsync()
    {
        if (TurnoActivo is null)
            await _turnoDatabase.IniciarTurnoAsync();
        else
            await _turnoDatabase.CerrarTurnoAsync(TurnoActivo);

        TurnoActivo = await _turnoDatabase.GetTurnoAbiertoAsync();
    }

}
