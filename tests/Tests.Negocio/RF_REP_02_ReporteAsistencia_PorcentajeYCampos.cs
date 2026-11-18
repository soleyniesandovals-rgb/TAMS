using Core.ControlAcceso.Domain.Enums;
using Tams.Negocio.Application.Modelos;
using Tams.Negocio.Domain.Enums;
using Tests.Negocio.Dobles;
using Xunit;

namespace Tests.Negocio;

/// <summary>
/// C-21 (RF-REP-02, RF-REP-03): una fila por asignación y estudiante con los
/// conteos y el porcentaje (presentes + tardanzas) / total × 100.
/// </summary>
public class RF_REP_02_ReporteAsistencia_PorcentajeYCampos
{
    [Fact]
    public async Task RF_REP_02_TresPresentesUnaTardanzaUnaAusencia_Porcentaje80ConTodosLosCampos()
    {
        // Preparar: Ana tiene 3 presentes, 1 tardanza y 1 ausencia; Luis, 1 excusa.
        await using var escenario = ContextoReporteAsistencia.Crear();
        await ContextoReporteAsistencia.SembrarEscenarioAsync(escenario.Contexto);
        await ContextoReporteAsistencia.SembrarAsistenciaAsync(escenario.Contexto, 1, 1, new DateOnly(2026, 10, 1), EstadoAsistencia.Presente);
        await ContextoReporteAsistencia.SembrarAsistenciaAsync(escenario.Contexto, 1, 1, new DateOnly(2026, 10, 2), EstadoAsistencia.Presente);
        await ContextoReporteAsistencia.SembrarAsistenciaAsync(escenario.Contexto, 1, 1, new DateOnly(2026, 10, 3), EstadoAsistencia.Presente);
        await ContextoReporteAsistencia.SembrarAsistenciaAsync(escenario.Contexto, 1, 1, new DateOnly(2026, 10, 4), EstadoAsistencia.Tardanza);
        await ContextoReporteAsistencia.SembrarAsistenciaAsync(escenario.Contexto, 1, 1, new DateOnly(2026, 10, 5), EstadoAsistencia.Ausente);
        await ContextoReporteAsistencia.SembrarAsistenciaAsync(escenario.Contexto, 1, 2, new DateOnly(2026, 10, 1), EstadoAsistencia.Excusa);

        // Actuar: el Administrador consulta el rango de octubre.
        var filas = await escenario.Servicio.ObtenerPorEstudianteYMateriaAsync(
            10, RolUsuario.Administrador, "2026-10-01", "2026-10-31", CancellationToken.None);

        // Verificar: 4/5 = 80.00; la excusa cuenta en el total pero no en el porcentaje;
        // el orden es materia, grado, sección, apellido, nombre.
        Assert.Equal(2, filas.Count);
        Assert.Equal(
            new AsistenciaEstudianteResumen("Matemática", "1er Grado", "A", "Ana", "Pérez", 3, 1, 1, 0, 5, 80.00m),
            filas.Single(f => f.Nombre == "Ana"));
        Assert.Equal(
            new AsistenciaEstudianteResumen("Matemática", "1er Grado", "A", "Luis", "Gómez", 0, 0, 0, 1, 1, 0.00m),
            filas.Single(f => f.Nombre == "Luis"));
        Assert.Equal(["Gómez", "Pérez"], filas.Select(f => f.Apellido).ToList());
    }
}
