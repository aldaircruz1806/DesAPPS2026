using System.Data;  
using UltimaMilla.Repartidor.ViewModels;
using UltimaMilla.Repartidor.Models;

namespace UltimaMilla.Repartidor.Views;

public partial class JornadaPage : ContentPage
{

    public JornadaPage(JornadaViewModel vm)
    {
        InitializeComponent();

        BindingContext = vm;
        FormatearFecha();

    }
    private void FormatearFecha()
    { 
        FechaLabel.Text = DateTime.Now.ToString("dddd d 'de' MMMM");

    }

    private async void OnParadaTapped(object sender, TappedEventArgs e)
    {
        if (sender is VisualElement { BindingContext: Parada parada })
        {
            await Shell.Current.GoToAsync($"{nameof(ParadaDetallePage)}?ParadaId={parada.Id}");
        }
    }


    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is JornadaViewModel vm)
        {
            await vm.RefrescaParadasCommand.ExecuteAsync(null);
            
        }
    }




}