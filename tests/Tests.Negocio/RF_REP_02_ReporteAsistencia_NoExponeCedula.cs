using Tams.Negocio.Application.Modelos;
using Xunit;

namespace Tests.Negocio;

/// <summary>
/// C-21 (RF-REP-02): el resultado del reporte no incluye la cédula del estudiante.
/// </summary>
public class RF_REP_02_ReporteAsistencia_NoExponeCedula
{
    [Fact]
    public void RF_REP_02_AsistenciaEstudianteResumen_NoTienePropiedadDeCedula()
    {
        // Actuar: se enumeran las propiedades públicas del DTO del reporte.
        var propiedades = typeof(AsistenciaEstudianteResumen)
            .GetProperties()
            .Select(p => p.Name)
            .ToList();

        // Verificar: no hay ninguna propiedad de cédula y solo se exponen los campos acordados.
        Assert.DoesNotContain(propiedades, nombre => nombre.Contains("Cedula", StringComparison.OrdinalIgnoreCase));
        Assert.Equal(
            [
                "MateriaNombre", "Grado", "Seccion", "Nombre", "Apellido",
                "Presentes", "Tardanzas", "Ausencias", "Excusas", "Total", "PorcentajeAsistencia",
            ],
            propiedades);
    }
}
