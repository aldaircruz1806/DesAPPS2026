namespace APIRest_min.Datos;

public class Turno
{
    public int Id { get; set; }
    public int RepartidorId { get; set; }
    public DateTime FechaInicio { get; set; }   // UTC, lo pone el servidor
    public DateTime? FechaFin { get; set; }     // null = turno abierto
    public double? LatitudInicio { get; set; }
    public double? LongitudInicio { get; set; }
}