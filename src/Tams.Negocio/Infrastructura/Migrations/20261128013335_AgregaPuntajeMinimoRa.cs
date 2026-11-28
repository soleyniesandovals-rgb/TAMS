using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tams.Negocio.Infrastructura.Migrations
{
    /// <inheritdoc />
    public partial class AgregaPuntajeMinimoRa : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PuntajeMinimo",
                schema: "negocio",
                table: "ResultadosAprendizaje",
                type: "int",
                nullable: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_ResultadoAprendizaje_PuntajeMinimo_Opcional",
                schema: "negocio",
                table: "ResultadosAprendizaje",
                sql: "[PuntajeMinimo] IS NULL OR ([PuntajeMinimo] BETWEEN 0 AND 100)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_ResultadoAprendizaje_PuntajeMinimo_Opcional",
                schema: "negocio",
                table: "ResultadosAprendizaje");

            migrationBuilder.DropColumn(
                name: "PuntajeMinimo",
                schema: "negocio",
                table: "ResultadosAprendizaje");
        }
    }
}
