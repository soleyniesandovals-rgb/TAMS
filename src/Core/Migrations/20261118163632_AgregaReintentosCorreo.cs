using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Core.Migrations
{
    /// <inheritdoc />
    public partial class AgregaReintentosCorreo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "FechaUltimoIntento",
                schema: "core",
                table: "CorreosEnCola",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Intentos",
                schema: "core",
                table: "CorreosEnCola",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "UltimoError",
                schema: "core",
                table: "CorreosEnCola",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FechaUltimoIntento",
                schema: "core",
                table: "CorreosEnCola");

            migrationBuilder.DropColumn(
                name: "Intentos",
                schema: "core",
                table: "CorreosEnCola");

            migrationBuilder.DropColumn(
                name: "UltimoError",
                schema: "core",
                table: "CorreosEnCola");
        }
    }
}
