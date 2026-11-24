using System.Net.Http.Json;
using UltimaMilla.Repartidor.Models;

namespace UltimaMilla.Repartidor.Services;

public class ApiTurnosService
{
    private readonly HttpClient _http;
    private readonly SesionActual _sesion;

    public ApiTurnosService(HttpClient http, SesionActual sesion)
    {
        _http = http;
        _sesion = sesion;
    }

    // Check-in: el RepartidorId sale de la sesion
    public async Task<Turno> AbrirTurnoAsync()
    {
        var nuevo = new Turno { RepartidorId = _sesion.RepartidorId };
        var resp = await _http.PostAsJsonAsync("api/turnos", nuevo);
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadFromJsonAsync<Turno>()
               ?? throw new InvalidOperationException("La API no devolvio el turno abierto.");
    }

    // Check-out: el id va en la ruta
    public async Task<Turno> CerrarTurnoAsync(int id)
    {
        var resp = await _http.PutAsync($"api/turnos/{id}/cerrar", null);
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadFromJsonAsync<Turno>()
               ?? throw new InvalidOperationException("La API no devolvio el turno cerrado.");
    }

    // Lectura: toma el repartidorId de la sesion
    public async Task<List<Turno>> ObtenerTurnosAsync()
        => await _http.GetFromJsonAsync<List<Turno>>($"api/turnos?repartidorId={_sesion.RepartidorId}")
           ?? new List<Turno>();

    // El turno abierto (FechaFin == null), si lo hay
    public async Task<Turno?> GetTurnoAbiertoAsync()
    {
        var turnos = await ObtenerTurnosAsync();
        return turnos.FirstOrDefault(t => t.FechaFin is null);
    }
}