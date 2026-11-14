using Tams.Negocio.Domain.Enums;

namespace Tams.Negocio.Application.Modelos;

/// <summary>
/// Datos de un curso para el Administrador, incluido el nombre del año escolar
/// al que pertenece.
/// </summary>
public record CursoResumen(
    int Id,
    string Grado,
    string Seccion,
    bool EsTecnico,
    TipoTecnico? TipoTecnico,
    int AnioEscolarId,
    string AnioEscolarNombre);
