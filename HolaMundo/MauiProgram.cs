using Microsoft.Extensions.Logging;
using UltimaMilla.Repartidor.Services;
using UltimaMilla.Repartidor.ViewModels;
using UltimaMilla.Repartidor.Views;

namespace HolaMundo
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            //La DB si es singleton porque se necesita una sola instancia para toda la app, no se crean nuevas instancias cada vez que se llama a la DB

            builder.Services.AddTransient<JornadaViewModel>();
            builder.Services.AddTransient<JornadaPage>();
            builder.Services.AddTransient<ParadaDetalleViewModel>();
            builder.Services.AddTransient<ParadaDetallePage>();

            builder.Services.AddSingleton<SesionActual>();
            builder.Services.AddSingleton<ParadasDatabase>();

            builder.Services.AddSingleton<RepartidorDatabase>();
            builder.Services.AddTransient<LoginViewModel>();
            builder.Services.AddTransient<LoginPage>();

            builder.Services.AddSingleton<TurnoDatabase>();

            builder.Services.AddTransient<HistoricoViewModel>();
            builder.Services.AddTransient<HistoricoPage>();


            return builder.Build();
        }
    }
}
