using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using UltimaMilla.Repartidor.Models;
using UltimaMilla.Repartidor.Services;
using RepartidorModel = global::UltimaMilla.Repartidor.Models.Repartidor;

namespace UltimaMilla.Repartidor.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly ApiRepartidoresService _apiRepartidores;
    private readonly RepartidorDatabase _repartidorDatabase;

    public ObservableCollection<RepartidorModel> Repartidores { get; } = new();

    public LoginViewModel(ApiRepartidoresService apiRepartidores, RepartidorDatabase repartidorDatabase)
    {
        _apiRepartidores = apiRepartidores;
        _repartidorDatabase = repartidorDatabase;
    }

    public async Task CargarAsync()
    {
        List<RepartidorModel> repartidores;
        try
        {
            repartidores = await _apiRepartidores.ObtenerRepartidoresAsync();
        }
        catch
        {
            // Si no hay red o falla la API, cae a la copia local en SQLite
            repartidores = await _repartidorDatabase.GetRepartidoresAsync();
        }

        Repartidores.Clear();
        foreach (var r in repartidores) Repartidores.Add(r);
    }
}