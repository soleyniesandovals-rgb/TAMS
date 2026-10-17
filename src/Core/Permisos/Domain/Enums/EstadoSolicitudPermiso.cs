namespace Core.Permisos.Domain.Enums;

/// <summary>
/// Estado de una solicitud de permiso (RF-GP-06). Sigue este ciclo de vida: nace
/// <see cref="Pendiente"/>; un administrador la resuelve a <see cref="Aprobada"/> o
/// <see cref="Rechazada"/>; una aprobada pasa a <see cref="Aplicada"/> cuando el
/// permiso queda otorgado (RF-GP-05).
/// Las transiciones válidas viven en
/// <see cref="Core.Permisos.Domain.Maquinas.TransicionesPermiso"/> (RD-04).
/// </summary>
public enum EstadoSolicitudPermiso
{
    /// <summary>Recién creada, a la espera de que un administrador la resuelva.</summary>
    Pendiente,

    /// <summary>Aprobada por un administrador; aún no se materializó el permiso.</summary>
    Aprobada,

    /// <summary>Rechazada por un administrador; estado terminal.</summary>
    Rechazada,

    /// <summary>El permiso quedó otorgado; estado terminal (RF-GP-05).</summary>
    Aplicada,
}
