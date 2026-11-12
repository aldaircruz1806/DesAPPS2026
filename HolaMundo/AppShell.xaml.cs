using UltimaMilla.Repartidor.Views;

namespace UltimaMilla.Repartidor
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute(
                nameof(ParadaDetallePage),
                typeof(ParadaDetallePage));
        }
        private async void OnCerrarSesionClicked(object sender, EventArgs e)
        {
            // Obtenemos el servicio inyectado manualmente porque MenuItem no hereda el BindingContext
            var sesion = Handler.MauiContext.Services.GetService<UltimaMilla.Repartidor.Services.SesionActual>();
            if (sesion != null)
            {
                sesion.RepartidorId = 0; // Solo borramos la sesión activa en memoria
            }
            await Shell.Current.GoToAsync("//LoginPage");
        }
    }

}