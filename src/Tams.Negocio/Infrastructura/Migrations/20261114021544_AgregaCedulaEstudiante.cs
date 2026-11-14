using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tams.Negocio.Infrastructura.Migrations
{
    /// <inheritdoc />
    public partial class AgregaCedulaEstudiante : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Cedula",
                schema: "negocio",
                table: "Estudiantes",
                type: "nvarchar(11)",
                maxLength: 11,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Estudiantes_Cedula",
                schema: "negocio",
                table: "Estudiantes",
                column: "Cedula",
                unique: true,
                filter: "[Cedula] IS NOT NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Estudiante_Cedula_OnceDigitos",
                schema: "negocio",
                table: "Estudiantes",
                sql: "[Cedula] IS NULL OR (LEN([Cedula]) = 11 AND [Cedula] NOT LIKE '%[^0-9]%')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Estudiantes_Cedula",
                schema: "negocio",
                table: "Estudiantes");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Estudiante_Cedula_OnceDigitos",
                schema: "negocio",
                table: "Estudiantes");

            migrationBuilder.DropColumn(
                name: "Cedula",
                schema: "negocio",
                table: "Estudiantes");
        }
    }
}
