using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using UltimaMilla.Repartidor.Models;
using UltimaMilla.Repartidor.Services;
using RepartidorModel = global::UltimaMilla.Repartidor.Models.Repartidor;
namespace UltimaMilla.Repartidor.ViewModels;

public partial class LoginViewModel : ObservableObject
{

    private readonly RepartidorDatabase _repartidorDatabase;

    public ObservableCollection<RepartidorModel> Repartidores { get; } = new();

    public LoginViewModel(RepartidorDatabase repartidorDatabase)
    {
        _repartidorDatabase = repartidorDatabase;
    }
    public async Task CargarAsync()
    {
        var repartidores = await _repartidorDatabase.GetRepartidoresAsync();
        Repartidores.Clear();
        foreach (var r in repartidores) Repartidores.Add(r);
    }

}
