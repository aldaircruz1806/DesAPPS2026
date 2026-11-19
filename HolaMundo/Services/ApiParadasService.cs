using System.Net.Http;
using System.Net.Http.Json;
using UltimaMilla.Repartidor.Models;


namespace UltimaMilla.Repartidor.Services;


public class ApiParadasService
{
    private readonly HttpClient _http;

    // La URL base y el timeout se configuran en MauiProgram (Parte 4).
    public ApiParadasService(HttpClient http) => _http = http;

    // Lista: "no hay nada" es un resultado normal -> lista vacia.
    public async Task<List<Parada>> ObtenerParadasAsync(int repartidorId)
    {
        var paradas = await _http.GetFromJsonAsync<List<Parada>>(
            $"api/paradas?repartidorId={repartidorId}");

        return paradas ?? new List<Parada>();
    }

    // Una parada: "no existe" es un resultado legitimo -> nulo.
    public async Task<Parada?> ObtenerParadaAsync(int id)
    {
        var respuesta = await _http.GetAsync($"api/paradas/{id}");

        if (respuesta.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;

        respuesta.EnsureSuccessStatusCode();
        return await respuesta.Content.ReadFromJsonAsync<Parada>();
    }

    // Escritura: el contrato promete devolver el objeto -> si no viene, es su falla.
    public async Task<Parada> ActualizarParadaAsync(Parada parada)
    {
        var respuesta = await _http.PutAsJsonAsync($"api/paradas/{parada.Id}", parada);
        respuesta.EnsureSuccessStatusCode();

        return await respuesta.Content.ReadFromJsonAsync<Parada>()
            ?? throw new InvalidOperationException(
                   "La API no devolvió la parada actualizada.");
    }

    public async Task<Parada> CrearParadaAsync(Parada parada)
    {
        var respuesta = await _http.PostAsJsonAsync("api/paradas", parada);
        respuesta.EnsureSuccessStatusCode();

        return await respuesta.Content.ReadFromJsonAsync<Parada>()
            ?? throw new InvalidOperationException(
                   "La API no devolvió la parada creada.");
    }
}
