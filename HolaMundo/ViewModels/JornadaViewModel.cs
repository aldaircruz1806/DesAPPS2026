using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using UltimaMilla.Repartidor.Models;
using UltimaMilla.Repartidor.Services;

namespace UltimaMilla.Repartidor.ViewModels;

public partial class JornadaViewModel : ObservableObject
{
    private readonly ParadasDatabase _paradasDatabase;
    private readonly RepartidorDatabase _repartidorDatabase;
    private readonly SesionActual _sesionActual;
    private readonly TurnoDatabase _turnoDatabase;

    //SE DECLARA UN TEMPORIZADOR PARA EL RETO 6.2
    private IDispatcherTimer _timer;

    //SE DECLARA TEMPORIZADOR PARA EL RETO 6.5
    private IDispatcherTimer _timerGps;

    // NUEVO EJERCICIO 6.5
    private readonly UbicacionService _ubicacionService;

    [ObservableProperty] private string _saludo = string.Empty;
    [ObservableProperty] private int _totalEntregas = 0;
    [ObservableProperty] private int _entregasCompletadas = 0;
    //RETO N2
    [ObservableProperty] private int _entregasPendientes = 0;
    [ObservableProperty] private bool _isRefrescando = false;
    [ObservableProperty] private string _mensajeEstado = string.Empty;
    [ObservableProperty] private bool _sinFiltro = false;
    [ObservableProperty] private Parada? _paradaSeleccionada;

    //SE DECLARA UN TEMPORIZADOR PARA EL RETO 6.2
    [ObservableProperty] private string _tiempoTranscurrido = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TextoBotonTurno))]
    private Turno? turnoActivo;

    public ObservableCollection<Parada> Paradas { get; } = new();

    public JornadaViewModel(ParadasDatabase paradasDatabase,
        RepartidorDatabase repartidorDatabase, SesionActual sesionActual, TurnoDatabase turnoDatabase, UbicacionService ubicacionService)
    {
        _paradasDatabase = paradasDatabase;
        _repartidorDatabase = repartidorDatabase;
        _sesionActual = sesionActual;
        _turnoDatabase = turnoDatabase;
        _ubicacionService = ubicacionService;

        //SE INICIALIZA EL TEMPORIZADOR PARA QUE SE DISPARE CADA MINUTO 6.2
        //SE USA IDDISPATCHERTIMER PARA ACTUALIZAR LA INTERFAZ DE MANERA SEGURA
        _timer = Application.Current.Dispatcher.CreateTimer();
        //SE CONFIGURA EL INTERVALO DE TIEMPO PARA QUE SE DISPARE CADA MINUTO
        _timer.Interval = TimeSpan.FromSeconds(1);
        //CADA QUE SE DISPARE EL TEMPORIZADOR MANDA A LLAMAR A CALCULAR TIEMPO
        _timer.Tick += (s, e) => CalcularTiempo();

        //SE INICIALIZA EL TEMPORIZADOR PARA ACTUALIZAR LA UBICACION
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

            var repartidor = await _repartidorDatabase.GetRepartidorAsync(_sesionActual.RepartidorId);

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

            TurnoActivo = await _turnoDatabase.GetTurnoAbiertoAsync();
            //RETO 6.2 REACTIVA EL RELOJ SI EL TURNO SOBREVIVIO
            if (TurnoActivo != null)
            {
                CalcularTiempo(); //ACTUALIZA EL TEXTO PARA QUE NO QUEDE EN BLANCO
                _timer.Start(); //ENCIENDE EL MOTOR DEL RELOJ
            }
            else
            {
                _timer.Stop();
                TiempoTranscurrido = string.Empty;
            }
#if DEBUG
            await Task.Delay(1000);
#endif

            var paradas = await _paradasDatabase.GetParadasAsync(true);

#if DEBUG
            if (paradas.Count == 0)
            {
                await InventaParadas();
                paradas = await _paradasDatabase.GetParadasAsync(true);
            }
#endif

            var miUbicacion = await _ubicacionService.ObtenerUbicacionActualAsync();

            if (miUbicacion == null)
            {
                MensajeEstado = "Activa la ubicación para ver distancias";
            }
            //RETO 6.5 DISTANCIAS
            //CALCULO DE DISTANCIAS
            //SE ITERA LA LISTA DE PARADAS DE LA BASE DE DATOS
            foreach (var p in paradas)
            {   //SE VERIFICA QUE EL DISPOSITIVO DIO LA UBICACION Y QUE TENGA LA PARADA COORDENADAS VALIDAS
                if (miUbicacion is not null && p.Latitud is not null && p.Longitud is not null)
                {
                    //SE CALCULA LA DISTANCIA EN LINEA RECTA USANDO LA FORMULA DE HAVERSINE
                    //EL RESULTADO SE GUARDA EN LA PROPIEDAD NUMERICA DISTANCIA KM QUE SE USARA PARA ORDENAR
                    p.DistanciaKm = UltimaMilla.Repartidor.Helpers.Distancia.EnKilometros(
                        miUbicacion.Latitude, miUbicacion.Longitude,
                        p.Latitud.Value, p.Longitud.Value);

                    p.DistanciaTexto = $"{p.DistanciaKm:F1} km"; //FORMATO VISUAL DE LOS KM CONVERSION DE DOUBLE A STRING CON REDONDEO
                }
                else
                { // SI EL GPS LLEGA A FALLAR SE ASIGNA DOUBLE.MAXVALUE PARA MANDAR EN EL ORDENAMIENTO LAS PARADAS AL FINAL DE LA LISTA
                  // Y QUE NO TRUENE LA APLICACION
                    p.DistanciaKm = double.MaxValue;
                    p.DistanciaTexto = "Sin ubicación";
                }
            }
            //SE USA EL METODO DE EXTENSION ORDERBY DE LINQ
            //TOMA LA LISTA, EVALUA EN DISTANCIAKM DE CADA OBJETO Y LA DEVUELVE LA LISTA ORDENADA DE MENOR A MAYOR
            paradas = paradas.OrderBy(p => p.DistanciaKm).ToList();

            TotalEntregas = paradas.Count;
            
            EntregasCompletadas = paradas.Count(p => p.Estado == EstadoEntrega.Entregada);
            //RETO N2 SE AGREGA UNA REGLA NUEVA PARA CONTAR LAS PARADAS EN PENDIENTE
            EntregasPendientes = paradas.Count(p => p.Estado == EstadoEntrega.Pendiente);
            ParadaSeleccionada = null;
            //LIMPIA LAS PARADAS DE LA OBSERVABLECOLLECTION
            Paradas.Clear();

            foreach (var parada in paradas)
            {
                //SE REORDENA LA LISTA PARADAS EN MEMORIA PARA DESPUES MANDARLA CON EL .ADD
                if (SinFiltro || parada.Estado != EstadoEntrega.Entregada)
                {
                    Paradas.Add(parada);
                }
            }

            if (string.IsNullOrEmpty(MensajeEstado) || MensajeEstado == "¡Datos actualizados correctamente!")
            {
                MensajeEstado = paradas.Count == 0
                    ? "No hay paradas disponibles."
                    : Paradas.Count == 0
                        ? "No hay paradas pendientes ni fallidas."
                        : "¡Datos actualizados correctamente!";
            }

            await Task.Delay(4000);
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
        await _paradasDatabase.SaveParadaAsync(new Parada
        {
            Numero = 1,
            RepartidorId = _sesionActual.RepartidorId,
            Cliente = "Farmacia San Juan",
            Direccion = "Av. Hidalgo 123",
            Estado = EstadoEntrega.Pendiente,
            Latitud = 25.4260,
            Longitud = -100.9920
        });

        await _paradasDatabase.SaveParadaAsync(new Parada
        {
            Numero = 2,
            RepartidorId = _sesionActual.RepartidorId,
            Cliente = "Abarrotes La Esquina",
            Direccion = "Calle Morelos 45",
            Estado = EstadoEntrega.Pendiente,
            Latitud = 25.4616,
            Longitud = -100.9782
        });

        await _paradasDatabase.SaveParadaAsync(new Parada
        {
            Numero = 3,
            RepartidorId = _sesionActual.RepartidorId,
            Cliente = "Panadería El Buen Pan",
            Direccion = "Calle Juárez 67",
            Estado = EstadoEntrega.Pendiente,
            Latitud = 25.4192,
            Longitud = -100.9416
        });

        
        await _paradasDatabase.SaveParadaAsync(new Parada
        {
            Numero = 4,
            RepartidorId = _sesionActual.RepartidorId,
            Cliente = "Don Panchito Perez",
            Direccion = "Calle Juárez 520",
            Estado = EstadoEntrega.Entregada,
            Latitud = 25.5410,
            Longitud = -100.9490
        });

        await _paradasDatabase.SaveParadaAsync(new Parada
        {
            Numero = 5,
            RepartidorId = _sesionActual.RepartidorId,
            Cliente = "Eulalio de la Cruz",
            Direccion = "Colon 888, Jalisco",
            Estado = EstadoEntrega.Entregada,
            Latitud = 25.4450,
            Longitud = -100.8520
        });
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
    private async Task AlternarTurnoAsync() //METODO MODIFICADO PARA EL RETO 6.2
    {
        if (TurnoActivo is null)
        {
            await _turnoDatabase.IniciarTurnoAsync();
            //EL RELOJ SOLO INICIA CUANDO SE INICIA EL TURNO
            _timer.Start();
        }
        else
        {
            await _turnoDatabase.CerrarTurnoAsync(TurnoActivo);
            //EL RELOJ SE DETIENE CUANDO SE CIERRA UN TURNO
            _timer.Stop();
            TiempoTranscurrido = string.Empty;
        }

        TurnoActivo = await _turnoDatabase.GetTurnoAbiertoAsync();
        CalcularTiempo();//FORZA UN CALCULO PARA NO ESPERAR 1 MINUTO EN QUE ACTUALICE
    }
    //RETO 6.2 TIEMPO ACTIVO EN EL TURNO
    private void CalcularTiempo()
    {
        //SE REVISA SI HAY UN TURNO ACTIVO Y QUE TENGA UNA FECHA DE INICIO VALIDA
        if (TurnoActivo?.FechaInicio != null)
        {
            //SE RESTA LA HORA ACTUAL DEL DATETIME.NOW - LA HORA EN QUE SE INICIO EL TURNO
            //GENERA UN OBJETO TIMESPAN LLAMADO DIFERENCIA QUE ES UN LAPSO DE TIEMPO
            var diferencia = DateTime.Now - TurnoActivo.FechaInicio;
            //SE LE DA FORMATO AL TIMESPAN PARA CALCULAR HORAS Y MINUTOS EXACTOS
            //COMO ESTA CREADA LA OBSERVABLEPROPERTY EN EL XAML SE ACTUALIZA SOLO
            TiempoTranscurrido = $"Tiempo Activo: {diferencia.Hours}h {diferencia.Minutes}m {diferencia.Seconds}s"
            ;
        }
    }
}