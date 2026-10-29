using UltimaMilla.Repartidor.ViewModels;

namespace UltimaMilla.Repartidor.Views;

public partial class ParadaDetallePage : ContentPage
{
	public ParadaDetallePage(ParadaDetalleViewModel vm)
	{
		InitializeComponent();
        BindingContext = vm;
    }
}