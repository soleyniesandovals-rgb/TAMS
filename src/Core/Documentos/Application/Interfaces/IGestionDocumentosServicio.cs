using Core.ControlAcceso.Domain.Enums;
using Core.Documentos.Application.Modelos;

namespace Core.Documentos.Application.Interfaces;

/// <summary>
/// Casos de uso de documentos (RF-DOC-01 a RF-DOC-04). Las reglas de acceso y
/// validación viven aquí, no en el controlador (RD-02, RD-06, RD-07).
/// </summary>
public interface IGestionDocumentosServicio
{
    Task<DocumentoResumen> SubirAsync(
        int usuarioId,
        string nombre,
        string tipoContenido,
        Stream contenido,
        long tamano,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DocumentoResumen>> ListarAsync(
        int usuarioId,
        RolUsuario rol,
        CancellationToken cancellationToken = default);

    Task<DocumentoDescarga> DescargarAsync(
        int usuarioId,
        RolUsuario rol,
        int id,
        CancellationToken cancellationToken = default);

    Task EliminarAsync(
        int usuarioId,
        RolUsuario rol,
        int id,
        CancellationToken cancellationToken = default);
}
