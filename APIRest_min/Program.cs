var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// ---------------------------------------------------------------------
// Datos en memoria. El Ejercicio 9 los mueve a PostgreSQL.
// Simulan lo que la torre de control ya asigno a cada repartidor.
// Los ids 1 y 2 corresponden a los dos repartidores sembrados en el
// Ejercicio 6.2 (Miguel Torres y Ana Delgado).
// ---------------------------------------------------------------------
var paradas = new List<Parada>
{
    // --- Repartidor 1 ---
    new() { Id = 1, RepartidorId = 1, Numero = 1,
            Cliente = "Farmacia San Juan",     Direccion = "Av. Hidalgo 123",
            Estado = EstadoEntrega.Entregada,
            Latitud = 20.6767, Longitud = -103.3475 },

    new() { Id = 2, RepartidorId = 1, Numero = 2,
            Cliente = "Abarrotes La Esquina",  Direccion = "Calle Morelos 45",
            Estado = EstadoEntrega.Pendiente,
            Latitud = 20.6700, Longitud = -103.3700 },

    new() { Id = 3, RepartidorId = 1, Numero = 3,
            Cliente = "Consultorio Dra. Ruiz", Direccion = "Blvd. Constitucion 890",
            Estado = EstadoEntrega.Pendiente,
            Latitud = 20.6600, Longitud = -103.3500 },

    new() { Id = 4, RepartidorId = 1, Numero = 4,
            Cliente = "Don Panchito Perez",    Direccion = "Ciudad Universitaria, Sistemas 890",
            Estado = EstadoEntrega.Pendiente,
            Latitud = 20.6550, Longitud = -103.3250 },
 
    // --- Repartidor 2 ---
    new() { Id = 5, RepartidorId = 2, Numero = 1,
            Cliente = "Clinica del Valle",     Direccion = "Av. Chapultepec 45",
            Estado = EstadoEntrega.Pendiente,
            Latitud = 20.6690, Longitud = -103.3690 },

    new() { Id = 6, RepartidorId = 2, Numero = 2,
            Cliente = "Eulalio de la Curze",   Direccion = "Colon 888, Jalisco",
            Estado = EstadoEntrega.Entregada,
            Latitud = 20.6620, Longitud = -103.3580 },
};

var siguienteId = 7;

// --- GET salud (Health Check) ---
app.MapGet("/api/salud", () => Results.Ok(new { estado = "ok", hora = DateTime.UtcNow }));


// --- GET todas, con filtro opcional por repartidor ------------------
app.MapGet("/api/paradas", (int? repartidorId) =>
    repartidorId is null
        ? paradas
        : paradas.Where(p => p.RepartidorId == repartidorId).ToList());

// --- GET resumen de paradas ---
app.MapGet("/api/paradas/resumen", (int? repartidorId) =>
{
    var paradasFiltradas = repartidorId is null
        ? paradas
        : paradas.Where(p => p.RepartidorId == repartidorId).ToList();

    var resumen = new
    {
        Total = paradasFiltradas.Count,
        Pendientes = paradasFiltradas.Count(p => p.Estado == EstadoEntrega.Pendiente),
        Entregadas = paradasFiltradas.Count(p => p.Estado == EstadoEntrega.Entregada),
        Fallidas = paradasFiltradas.Count(p => p.Estado == EstadoEntrega.Fallida)
    };

    return Results.Ok(resumen);
});

// --- GET una ---------------------------------------------------------
app.MapGet("/api/paradas/{id:int}", (int id) =>
{
    var parada = paradas.FirstOrDefault(p => p.Id == id);
    return parada is null ? Results.NotFound() : Results.Ok(parada);
});

// --- POST crear (Con validación) ---
app.MapPost("/api/paradas", (Parada nueva) =>
{
    // Validación: Si el cliente o la dirección vienen vacíos, regresa 400 Bad Request
    if (string.IsNullOrWhiteSpace(nueva.Cliente) || string.IsNullOrWhiteSpace(nueva.Direccion))
    {
        return Results.BadRequest(new { error = "El nombre del cliente y la dirección son obligatorios." });
    }

    nueva.Id = siguienteId++;
    nueva.ModificadoEn = DateTime.UtcNow;
    paradas.Add(nueva);

    return Results.Created($"/api/paradas/{nueva.Id}", nueva);
});

// --- PUT actualizar --------------------------------------------------
app.MapPut("/api/paradas/{id:int}", (int id, Parada cambios) =>
{
    var actual = paradas.FirstOrDefault(p => p.Id == id);
    if (actual is null) return Results.NotFound();

    actual.Numero = cambios.Numero;
    actual.Cliente = cambios.Cliente;
    actual.Direccion = cambios.Direccion;
    actual.Estado = cambios.Estado;
    actual.RazonFallo = cambios.RazonFallo;
    actual.Latitud = cambios.Latitud;
    actual.Longitud = cambios.Longitud;
    actual.ModificadoEn = DateTime.UtcNow;  // la hora la pone el servidor

    return Results.Ok(actual);              // el contrato exige devolver el objeto
});

// --- DELETE borrar ---------------------------------------------------
app.MapDelete("/api/paradas/{id:int}", (int id) =>
{
    var actual = paradas.FirstOrDefault(p => p.Id == id);
    if (actual is null) return Results.NotFound();

    paradas.Remove(actual);
    return Results.NoContent();
});

app.Run();

// ---------------------------------------------------------------------
// El modelo del servidor. Mismo contrato que el de MAUI, pero SIN los
// atributos de sqlite-net: aqui no hay SQLite, y el servidor no debe
// saber como el cliente guarda sus datos.
// ---------------------------------------------------------------------
public class Parada
{
    public int Id { get; set; }
    public int RepartidorId { get; set; }
    public int Numero { get; set; }
    public string Cliente { get; set; } = "";
    public string Direccion { get; set; } = "";
    public EstadoEntrega Estado { get; set; } = EstadoEntrega.Pendiente;
    public string? RazonFallo { get; set; }
    public DateTime ModificadoEn { get; set; } = DateTime.UtcNow;
    public double? Latitud { get; set; }
    public double? Longitud { get; set; }
}

public enum EstadoEntrega { Pendiente, Entregada, Fallida }
