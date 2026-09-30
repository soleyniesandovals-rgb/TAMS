using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Core.Migrations
{
    /// <inheritdoc />
    public partial class InicialCore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "core");

            migrationBuilder.CreateTable(
                name: "CorreosEnCola",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Destinatario = table.Column<string>(type: "nvarchar(320)", maxLength: 320, nullable: false),
                    Asunto = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Cuerpo = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Pendiente"),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaEnvio = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CorreosEnCola", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Correo = table.Column<string>(type: "nvarchar(320)", maxLength: 320, nullable: false),
                    ContraseñaHash = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Rol = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TokenActivaciones",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    Codigo = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    FechaEmision = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaVencimiento = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Usado = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TokenActivaciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TokenActivaciones_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalSchema: "core",
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CorreosEnCola_Estado",
                schema: "core",
                table: "CorreosEnCola",
                column: "Estado");

            migrationBuilder.CreateIndex(
                name: "IX_TokenActivaciones_Codigo",
                schema: "core",
                table: "TokenActivaciones",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TokenActivaciones_UsuarioId",
                schema: "core",
                table: "TokenActivaciones",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_Correo",
                schema: "core",
                table: "Usuarios",
                column: "Correo",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CorreosEnCola",
                schema: "core");

            migrationBuilder.DropTable(
                name: "TokenActivaciones",
                schema: "core");

            migrationBuilder.DropTable(
                name: "Usuarios",
                schema: "core");
        }
    }
}
