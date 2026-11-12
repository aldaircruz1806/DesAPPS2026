using SQLite;

namespace UltimaMilla.Repartidor.Models
{
    public class Parada
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public int Numero { get; set; }  //Orden de visita
        public int RepartidorId { get; set; } //Id del repartidor
        public string Cliente { get; set; } = "";//Nombre del cliente
        public string Direccion { get; set; } = "";//Direccion del cliente
        public EstadoEntrega Estado { get; set; } = EstadoEntrega.Pendiente;
        public string? RazonFallo { get; set; } //Razon de fallo en caso de no entregar
        public DateTime ModificadoEn { get; set; } = DateTime.Now;
        //SE AGREGAN CAMPOS OPCIONALES A PARADA 6.5
        public double? Latitud { get; set; }
        public double? Longitud { get; set; }

        [Ignore]
        public string DistanciaTexto { get; set; } = "";
        //RETO 6.5
        [Ignore]
        public double DistanciaKm { get; set; } = double.MaxValue; // Valor alto por defecto para mandar las que no tienen GPS al fondo

    }
}
