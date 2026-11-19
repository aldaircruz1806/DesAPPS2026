using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace APIRest_min.Migrations
{
    /// <inheritdoc />
    public partial class Inicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Paradas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RepartidorId = table.Column<int>(type: "integer", nullable: false),
                    Numero = table.Column<int>(type: "integer", nullable: false),
                    Cliente = table.Column<string>(type: "text", nullable: false),
                    Direccion = table.Column<string>(type: "text", nullable: false),
                    Estado = table.Column<int>(type: "integer", nullable: false),
                    RazonFallo = table.Column<string>(type: "text", nullable: true),
                    ModificadoEn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Latitud = table.Column<double>(type: "double precision", nullable: true),
                    Longitud = table.Column<double>(type: "double precision", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Paradas", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Paradas",
                columns: new[] { "Id", "Cliente", "Direccion", "Estado", "Latitud", "Longitud", "ModificadoEn", "Numero", "RazonFallo", "RepartidorId" },
                values: new object[,]
                {
                    { 1, "Farmacia San Juan", "Av. Hidalgo 123", 1, 20.6767, -103.3475, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, null, 1 },
                    { 2, "Abarrotes La Esquina", "Calle Morelos 45", 0, 20.670000000000002, -103.37, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, null, 1 },
                    { 3, "Clinica del Valle", "Av. Chapultepec 45", 0, 20.669, -103.369, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, null, 2 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Paradas");
        }
    }
}
