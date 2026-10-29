namespace Tams.Negocio.Application;

/// <summary>
/// Tipos de permiso que el módulo de negocio consulta a Gestión de permisos del Core
/// (RF-NEG-06). El tipo es un texto acordado entre ambos módulos.
/// </summary>
public static class TiposPermiso
{
    /// <summary>Permite corregir el puntaje de una calificación publicada (RF-NEG-02).</summary>
    public const string CorregirCalificacion = "corregir-calificacion";
}
