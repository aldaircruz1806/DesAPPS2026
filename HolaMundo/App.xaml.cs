using Microsoft.Extensions.DependencyInjection;
using UltimaMilla.Repartidor;

namespace UltimaMilla.Repartidor
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new SplashPage());
        }

    }
}