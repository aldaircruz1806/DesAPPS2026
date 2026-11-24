namespace APIRest_min.Datos;

public class Repartidor
{
    public int Id { get; set; }
    public string Usuario { get; set; } = "";   // lo usará el login del Ej. 10
    public bool Activo { get; set; } = true;
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public DateTime FechaRegistro { get; set; } // UTC; fija en el seed
}