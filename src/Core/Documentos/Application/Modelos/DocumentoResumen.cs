namespace Core.Documentos.Application.Modelos;

/// <summary>
/// Datos públicos de un documento (RF-DOC-01 a RF-DOC-04). Nunca incluye la
/// ruta física de almacenamiento.
/// </summary>
public sealed record DocumentoResumen(
    int Id,
    string Nombre,
    string TipoContenido,
    long TamanoBytes,
    DateTime FechaSubida,
    int UsuarioId);
