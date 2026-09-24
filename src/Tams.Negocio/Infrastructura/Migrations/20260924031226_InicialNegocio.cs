using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tams.Negocio.Infrastructura.Migrations
{
    /// <inheritdoc />
    public partial class InicialNegocio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "negocio");

            migrationBuilder.CreateTable(
                name: "AniosEscolares",
                schema: "negocio",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    FechaInicio = table.Column<DateOnly>(type: "date", nullable: false),
                    FechaFin = table.Column<DateOnly>(type: "date", nullable: false),
                    EsActivo = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AniosEscolares", x => x.Id);
                    table.CheckConstraint("CK_AnioEscolar_FechaFin_MayorQue_FechaInicio", "[FechaFin] > [FechaInicio]");
                });

            migrationBuilder.CreateTable(
                name: "Centros",
                schema: "negocio",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    TipoCentro = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TiposTecnicosHabilitados = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Centros", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Materias",
                schema: "negocio",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    EsTecnica = table.Column<bool>(type: "bit", nullable: false),
                    TipoTecnico = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CantidadRA = table.Column<int>(type: "int", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Materias", x => x.Id);
                    table.CheckConstraint("CK_Materia_CantidadRA_Positiva", "[CantidadRA] IS NULL OR [CantidadRA] > 0");
                    table.CheckConstraint("CK_Materia_CantidadRA_SoloSiEsTecnica", "[CantidadRA] IS NULL OR [EsTecnica] = 1");
                    table.CheckConstraint("CK_Materia_TipoTecnico_SoloSiEsTecnica", "[TipoTecnico] IS NULL OR [EsTecnica] = 1");
                });

            migrationBuilder.CreateTable(
                name: "Cursos",
                schema: "negocio",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Grado = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Seccion = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    EsTecnico = table.Column<bool>(type: "bit", nullable: false),
                    TipoTecnico = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    AnioEscolarId = table.Column<int>(type: "int", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cursos", x => x.Id);
                    table.CheckConstraint("CK_Curso_TipoTecnico_SoloSiEsTecnico", "[TipoTecnico] IS NULL OR [EsTecnico] = 1");
                    table.ForeignKey(
                        name: "FK_Cursos_AniosEscolares_AnioEscolarId",
                        column: x => x.AnioEscolarId,
                        principalSchema: "negocio",
                        principalTable: "AniosEscolares",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AsignacionesDocentes",
                schema: "negocio",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    MateriaId = table.Column<int>(type: "int", nullable: false),
                    CursoId = table.Column<int>(type: "int", nullable: false),
                    HorasSemanales = table.Column<int>(type: "int", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AsignacionesDocentes", x => x.Id);
                    table.CheckConstraint("CK_AsignacionDocente_HorasSemanales_Positivas", "[HorasSemanales] > 0");
                    table.ForeignKey(
                        name: "FK_AsignacionesDocentes_Cursos_CursoId",
                        column: x => x.CursoId,
                        principalSchema: "negocio",
                        principalTable: "Cursos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AsignacionesDocentes_Materias_MateriaId",
                        column: x => x.MateriaId,
                        principalSchema: "negocio",
                        principalTable: "Materias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Estudiantes",
                schema: "negocio",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Apellido = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CursoId = table.Column<int>(type: "int", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Estudiantes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Estudiantes_Cursos_CursoId",
                        column: x => x.CursoId,
                        principalSchema: "negocio",
                        principalTable: "Cursos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BloquesHorarios",
                schema: "negocio",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AsignacionDocenteId = table.Column<int>(type: "int", nullable: false),
                    DiaSemana = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    HoraInicio = table.Column<TimeOnly>(type: "time", nullable: false),
                    HoraFin = table.Column<TimeOnly>(type: "time", nullable: false),
                    Bloqueado = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BloquesHorarios", x => x.Id);
                    table.CheckConstraint("CK_BloqueHorario_HoraFin_MayorQue_HoraInicio", "[HoraFin] > [HoraInicio]");
                    table.ForeignKey(
                        name: "FK_BloquesHorarios_AsignacionesDocentes_AsignacionDocenteId",
                        column: x => x.AsignacionDocenteId,
                        principalSchema: "negocio",
                        principalTable: "AsignacionesDocentes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Calificaciones",
                schema: "negocio",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EstudianteId = table.Column<int>(type: "int", nullable: false),
                    MateriaId = table.Column<int>(type: "int", nullable: false),
                    AnioEscolarId = table.Column<int>(type: "int", nullable: false),
                    TipoEvaluacion = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Periodo = table.Column<int>(type: "int", nullable: true),
                    NumeroRA = table.Column<int>(type: "int", nullable: true),
                    PuntajeObtenido = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PuntajeRecuperacion = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, defaultValue: "Borrador"),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Calificaciones", x => x.Id);
                    table.CheckConstraint("CK_Calificacion_PuntajeObtenido_NoNegativo", "[PuntajeObtenido] >= 0");
                    table.CheckConstraint("CK_Calificacion_PuntajeRecuperacion_NoNegativo", "[PuntajeRecuperacion] IS NULL OR [PuntajeRecuperacion] >= 0");
                    table.ForeignKey(
                        name: "FK_Calificaciones_AniosEscolares_AnioEscolarId",
                        column: x => x.AnioEscolarId,
                        principalSchema: "negocio",
                        principalTable: "AniosEscolares",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Calificaciones_Estudiantes_EstudianteId",
                        column: x => x.EstudianteId,
                        principalSchema: "negocio",
                        principalTable: "Estudiantes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Calificaciones_Materias_MateriaId",
                        column: x => x.MateriaId,
                        principalSchema: "negocio",
                        principalTable: "Materias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AniosEscolares_EsActivo",
                schema: "negocio",
                table: "AniosEscolares",
                column: "EsActivo",
                unique: true,
                filter: "[EsActivo] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_AniosEscolares_Nombre",
                schema: "negocio",
                table: "AniosEscolares",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AsignacionesDocentes_CursoId",
                schema: "negocio",
                table: "AsignacionesDocentes",
                column: "CursoId");

            migrationBuilder.CreateIndex(
                name: "IX_AsignacionesDocentes_MateriaId_CursoId",
                schema: "negocio",
                table: "AsignacionesDocentes",
                columns: new[] { "MateriaId", "CursoId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BloquesHorarios_AsignacionDocenteId",
                schema: "negocio",
                table: "BloquesHorarios",
                column: "AsignacionDocenteId");

            migrationBuilder.CreateIndex(
                name: "IX_Calificaciones_AnioEscolarId",
                schema: "negocio",
                table: "Calificaciones",
                column: "AnioEscolarId");

            migrationBuilder.CreateIndex(
                name: "IX_Calificaciones_EstudianteId",
                schema: "negocio",
                table: "Calificaciones",
                column: "EstudianteId");

            migrationBuilder.CreateIndex(
                name: "IX_Calificaciones_MateriaId",
                schema: "negocio",
                table: "Calificaciones",
                column: "MateriaId");

            migrationBuilder.CreateIndex(
                name: "IX_Cursos_AnioEscolarId_Grado_Seccion",
                schema: "negocio",
                table: "Cursos",
                columns: new[] { "AnioEscolarId", "Grado", "Seccion" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Estudiantes_CursoId",
                schema: "negocio",
                table: "Estudiantes",
                column: "CursoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BloquesHorarios",
                schema: "negocio");

            migrationBuilder.DropTable(
                name: "Calificaciones",
                schema: "negocio");

            migrationBuilder.DropTable(
                name: "Centros",
                schema: "negocio");

            migrationBuilder.DropTable(
                name: "AsignacionesDocentes",
                schema: "negocio");

            migrationBuilder.DropTable(
                name: "Estudiantes",
                schema: "negocio");

            migrationBuilder.DropTable(
                name: "Materias",
                schema: "negocio");

            migrationBuilder.DropTable(
                name: "Cursos",
                schema: "negocio");

            migrationBuilder.DropTable(
                name: "AniosEscolares",
                schema: "negocio");
        }
    }
}
