using System.Net.Http.Json;
using UltimaMilla.Repartidor.Models;
using RepartidorModel = global::UltimaMilla.Repartidor.Models.Repartidor;
namespace UltimaMilla.Repartidor.Services;

public class ApiRepartidoresService
{
    private readonly HttpClient _http;

    public ApiRepartidoresService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<RepartidorModel>> ObtenerRepartidoresAsync()
    {
        var lista = await _http.GetFromJsonAsync<List<RepartidorModel>>("api/repartidores");
        return lista ?? throw new InvalidOperationException("La API no devolvio repartidores.");
    }
}