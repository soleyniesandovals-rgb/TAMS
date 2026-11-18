using Core.ControlAcceso.Domain.Enums;
using Tams.Negocio.Domain.Enums;
using Tests.Negocio.Dobles;
using Xunit;

namespace Tests.Negocio;

/// <summary>
/// C-24 (RF-REP-05): en el reporte de asistencia, la fecha igual a "hasta"
/// cuenta y la del día siguiente no.
/// </summary>
public class RF_REP_05_ReporteAsistencia_UltimoDiaIncluido
{
    [Fact]
    public async Task RF_REP_05_AsistenciaDelUltimoDia_CuentaYLaDelDiaSiguienteNo()
    {
        // Preparar: registros el 31 de octubre y el 1 de noviembre.
        await using var escenario = ContextoReporteAsistencia.Crear();
        await ContextoReporteAsistencia.SembrarEscenarioAsync(escenario.Contexto);
        await ContextoReporteAsistencia.SembrarAsistenciaAsync(escenario.Contexto, 1, 1, new DateOnly(2026, 10, 31), EstadoAsistencia.Presente);
        await ContextoReporteAsistencia.SembrarAsistenciaAsync(escenario.Contexto, 1, 1, new DateOnly(2026, 11, 1), EstadoAsistencia.Ausente);

        // Actuar: el rango termina el 31 de octubre.
        var filas = await escenario.Servicio.ObtenerPorEstudianteYMateriaAsync(
            10, RolUsuario.Administrador, "2026-10-01", "2026-10-31", CancellationToken.None);

        // Verificar: solo cuenta la del último día; total 1 y porcentaje 100.
        var fila = Assert.Single(filas);
        Assert.Equal(1, fila.Total);
        Assert.Equal(1, fila.Presentes);
        Assert.Equal(0, fila.Ausencias);
        Assert.Equal(100.00m, fila.PorcentajeAsistencia);
    }
}
