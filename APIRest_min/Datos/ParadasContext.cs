using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Numerics;

namespace APIRest_min.Datos;

public class ParadasContext : DbContext
{
    public ParadasContext(DbContextOptions<ParadasContext> opciones)
        : base(opciones) { }

    public DbSet<Parada> Paradas => Set<Parada>();
    public DbSet<Repartidor> Repartidores => Set<Repartidor>();
    public DbSet<Turno> Turnos => Set<Turno>();

    protected override void OnModelCreating(ModelBuilder modelo)
    {
        // El sembrado deja de vivir en memoria: entra a la migración
        // y nace dentro de la base, igual que la tabla.
        modelo.Entity<Parada>().HasData(
            new Parada
            {
                Id = 1,
                RepartidorId = 1,
                Numero = 1,
                Cliente = "Farmacia San Juan",
                Direccion = "Av. Hidalgo 123",
                Estado = EstadoEntrega.Entregada,
                Latitud = 20.6767,
                Longitud = -103.3475,
                ModificadoEn = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Parada
            {
                Id = 2,
                RepartidorId = 1,
                Numero = 2,
                Cliente = "Abarrotes La Esquina",
                Direccion = "Calle Morelos 45",
                Estado = EstadoEntrega.Pendiente,
                Latitud = 20.6700,
                Longitud = -103.3700,
                ModificadoEn = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Parada
            {
                Id = 3,
                RepartidorId = 2,
                Numero = 1,
                Cliente = "Clinica del Valle",
                Direccion = "Av. Chapultepec 45",
                Estado = EstadoEntrega.Pendiente,
                Latitud = 20.6690,
                Longitud = -103.3690,
                ModificadoEn = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        );

        modelo.Entity<Repartidor>().HasData(
            new Repartidor
            {
                Id = 1,
                Nombre = "Miguel",
                Apellido = "Torres",
                Usuario = "miguel",
                Email = "miguel.torres@example.com",
                Telefono = "123456789",
                FechaRegistro = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Repartidor
            {
                Id = 2,
                Nombre = "Ana",
                Apellido = "Delgado",
                Usuario = "ana",
                Email = "ana.delgado@example.com",
                Telefono = "987654321",
                FechaRegistro = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Repartidor
            {
                Id = 3,
                Nombre = "Carlos",
                Apellido = "Ramirez",
                Usuario = "carlos",
                Email = "carlos.ramirez@example.com",
                Telefono = "555555555",
                FechaRegistro = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        );
    }
}
