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
        public string Estado { get; set; } = "Pendiente";    
        public string? RazonFallo { get; set; } //Razon de fallo en caso de no entregar
        public DateTime ModificadoEn { get; set; } = DateTime.Now;
    }
}
