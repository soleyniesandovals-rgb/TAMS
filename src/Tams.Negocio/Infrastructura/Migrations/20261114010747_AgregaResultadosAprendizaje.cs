using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tams.Negocio.Infrastructura.Migrations
{
    /// <inheritdoc />
    public partial class AgregaResultadosAprendizaje : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ResultadosAprendizaje",
                schema: "negocio",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MateriaId = table.Column<int>(type: "int", nullable: false),
                    Numero = table.Column<int>(type: "int", nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Puntos = table.Column<int>(type: "int", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResultadosAprendizaje", x => x.Id);
                    table.CheckConstraint("CK_ResultadoAprendizaje_Numero_Positivo", "[Numero] >= 1");
                    table.CheckConstraint("CK_ResultadoAprendizaje_Puntos_EntreUnoYCien", "[Puntos] BETWEEN 1 AND 100");
                    table.ForeignKey(
                        name: "FK_ResultadosAprendizaje_Materias_MateriaId",
                        column: x => x.MateriaId,
                        principalSchema: "negocio",
                        principalTable: "Materias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ResultadosAprendizaje_MateriaId_Numero",
                schema: "negocio",
                table: "ResultadosAprendizaje",
                columns: new[] { "MateriaId", "Numero" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ResultadosAprendizaje",
                schema: "negocio");
        }
    }
}
