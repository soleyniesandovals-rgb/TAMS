using Core.ControlAcceso.Domain.Enums;
using Tams.Negocio.Domain.Enums;
using Tests.Negocio.Dobles;
using Xunit;

namespace Tests.Negocio;

/// <summary>
/// C-22 (RF-REP-04): el docente Estándar ve solo sus asignaciones; el
/// Administrador ve todas.
/// </summary>
public class RF_REP_04_ReporteAsistencia_FiltroPorRol
{
    [Fact]
    public async Task RF_REP_04_DocenteVeSoloSusAsignacionesYAdministradorVeTodas()
    {
        // Preparar: una asignación del docente 10 y otra del docente 20.
        await using var escenario = ContextoReporteAsistencia.Crear();
        await ContextoReporteAsistencia.SembrarEscenarioAsync(escenario.Contexto);
        await ContextoReporteAsistencia.SembrarAsistenciaAsync(escenario.Contexto, 1, 1, new DateOnly(2026, 10, 6), EstadoAsistencia.Presente);
        await ContextoReporteAsistencia.SembrarAsistenciaAsync(escenario.Contexto, 2, 3, new DateOnly(2026, 10, 6), EstadoAsistencia.Presente);

        // Actuar: consulta el docente 10 y, después, un Administrador.
        var propias = await escenario.Servicio.ObtenerPorEstudianteYMateriaAsync(
            10, RolUsuario.Estandar, "2026-10-01", "2026-10-31", CancellationToken.None);
        var todas = await escenario.Servicio.ObtenerPorEstudianteYMateriaAsync(
            10, RolUsuario.Administrador, "2026-10-01", "2026-10-31", CancellationToken.None);

        // Verificar: el docente ve solo "Ana" (su asignación 1); el Administrador ve también a "Marta".
        Assert.Equal(["Pérez"], propias.Select(f => f.Apellido).ToList());
        Assert.Equal(["Díaz", "Pérez"], todas.Select(f => f.Apellido).ToList());
        Assert.All(propias, fila => Assert.Equal("Matemática", fila.MateriaNombre));
    }
}
