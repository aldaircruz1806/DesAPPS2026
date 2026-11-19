using Microsoft.EntityFrameworkCore;
using APIRest_min.Datos;

var builder = WebApplication.CreateBuilder(args);

// La cadena NUNCA va escrita aquí: viene de la configuración.
var nombreBase = builder.Configuration["BaseActiva"] ?? "Paradas";
var cadena = builder.Configuration.GetConnectionString(nombreBase)
    ?? throw new InvalidOperationException(
           $"Falta la cadena de conexion '{nombreBase}'. Revisa user-secrets o las variables de entorno.");

builder.Services.AddDbContext<ParadasContext>(opciones =>
    opciones.UseNpgsql(cadena));

var app = builder.Build();

// --- GET salud (Health Check) ---
app.MapGet("/api/salud", () => Results.Ok(new { estado = "ok", hora = DateTime.UtcNow }));

// --- GET todas, con filtro opcional por repartidor ------------------
app.MapGet("/api/paradas", async (int? repartidorId, ParadasContext db) =>
{
    var consulta = db.Paradas.AsQueryable();

    if (repartidorId is not null)
        consulta = consulta.Where(p => p.RepartidorId == repartidorId);

    return await consulta.OrderBy(p => p.Numero).ToListAsync();
});

// --- GET resumen de paradas ---
app.MapGet("/api/paradas/resumen", async (int? repartidorId, ParadasContext db) =>
{
    var consulta = db.Paradas.AsQueryable();

    if (repartidorId is not null)
        consulta = consulta.Where(p => p.RepartidorId == repartidorId);

    var paradasFiltradas = await consulta.ToListAsync();

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
app.MapGet("/api/paradas/{id:int}", async (int id, ParadasContext db) =>
{
    var parada = await db.Paradas.FindAsync(id);
    return parada is null ? Results.NotFound() : Results.Ok(parada);
});

// --- POST crear (Con validación) ---
app.MapPost("/api/paradas", async (Parada nueva, ParadasContext db) =>
{
    if (string.IsNullOrWhiteSpace(nueva.Cliente) || string.IsNullOrWhiteSpace(nueva.Direccion))
    {
        return Results.BadRequest(new { error = "El nombre del cliente y la dirección son obligatorios." });
    }

    nueva.ModificadoEn = DateTime.UtcNow;
    db.Paradas.Add(nueva);
    await db.SaveChangesAsync();

    return Results.Created($"/api/paradas/{nueva.Id}", nueva);
});

// --- PUT actualizar --------------------------------------------------
app.MapPut("/api/paradas/{id:int}", async (int id, Parada cambios, ParadasContext db) =>
{
    var actual = await db.Paradas.FindAsync(id);
    if (actual is null) return Results.NotFound();

    actual.Numero = cambios.Numero;
    actual.Cliente = cambios.Cliente;
    actual.Direccion = cambios.Direccion;
    actual.Estado = cambios.Estado;
    actual.RazonFallo = cambios.RazonFallo;
    actual.Latitud = cambios.Latitud;
    actual.Longitud = cambios.Longitud;
    actual.ModificadoEn = DateTime.UtcNow;

    await db.SaveChangesAsync();
    return Results.Ok(actual);
});

// --- DELETE borrar ---------------------------------------------------
app.MapDelete("/api/paradas/{id:int}", async (int id, ParadasContext db) =>
{
    var actual = await db.Paradas.FindAsync(id);
    if (actual is null) return Results.NotFound();

    db.Paradas.Remove(actual);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.Run();

// ---------------------------------------------------------------------
// El modelo del servidor.
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