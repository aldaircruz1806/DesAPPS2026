using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace APIRest_min.Migrations
{
    /// <inheritdoc />
    public partial class RepartidoresYTurnos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Repartidores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Usuario = table.Column<string>(type: "text", nullable: false),
                    Activo = table.Column<bool>(type: "boolean", nullable: false),
                    Nombre = table.Column<string>(type: "text", nullable: false),
                    Apellido = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    Telefono = table.Column<string>(type: "text", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Repartidores", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Turnos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RepartidorId = table.Column<int>(type: "integer", nullable: false),
                    FechaInicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FechaFin = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LatitudInicio = table.Column<double>(type: "double precision", nullable: true),
                    LongitudInicio = table.Column<double>(type: "double precision", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Turnos", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Repartidores",
                columns: new[] { "Id", "Activo", "Apellido", "Email", "FechaRegistro", "Nombre", "Telefono", "Usuario" },
                values: new object[,]
                {
                    { 1, true, "Torres", "miguel.torres@example.com", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Miguel", "123456789", "miguel" },
                    { 2, true, "Delgado", "ana.delgado@example.com", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Ana", "987654321", "ana" },
                    { 3, true, "Ramirez", "carlos.ramirez@example.com", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Carlos", "555555555", "carlos" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Repartidores");

            migrationBuilder.DropTable(
                name: "Turnos");
        }
    }
}
