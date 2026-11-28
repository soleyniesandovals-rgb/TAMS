using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tams.Negocio.Infrastructura.Migrations
{
    /// <inheritdoc />
    public partial class AgregaEventosCalendario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EventosCalendario",
                schema: "negocio",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AnioEscolarId = table.Column<int>(type: "int", nullable: false),
                    Fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CreadoPorUsuarioId = table.Column<int>(type: "int", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventosCalendario", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EventosCalendario_AniosEscolares_AnioEscolarId",
                        column: x => x.AnioEscolarId,
                        principalSchema: "negocio",
                        principalTable: "AniosEscolares",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EventosCalendario_AnioEscolarId_Fecha",
                schema: "negocio",
                table: "EventosCalendario",
                columns: new[] { "AnioEscolarId", "Fecha" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EventosCalendario",
                schema: "negocio");
        }
    }
}
