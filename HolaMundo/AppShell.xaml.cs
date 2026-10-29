using UltimaMilla.Repartidor.Views;

namespace HolaMundo
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
    }
}