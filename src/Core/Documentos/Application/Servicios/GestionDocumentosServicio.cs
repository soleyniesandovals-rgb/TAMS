using Core.ControlAcceso.Application.Interfaces;
using Core.ControlAcceso.Domain.Enums;
using Core.ControlAcceso.Domain.Excepciones;
using Core.Documentos.Application.Interfaces;
using Core.Documentos.Application.Modelos;
using Core.Documentos.Domain.Entidades;

namespace Core.Documentos.Application.Servicios;

/// <summary>
/// Casos de uso de documentos (RF-DOC-01 a RF-DOC-04).
/// RD-02: toda la lógica de negocio vive aquí; el controlador solo traduce a HTTP.
/// RD-07: la entrada se valida antes de usarse.
/// RD-08: los mensajes son controlados y nunca incluyen rutas ni trazas.
/// RD-11: la fecha de subida se toma del <see cref="TimeProvider"/> inyectado.
/// </summary>
public class GestionDocumentosServicio(
    ICoreUnidadDeTrabajo unidad,
    IAlmacenamientoArchivos almacenamiento,
    TimeProvider timeProvider) : IGestionDocumentosServicio
{
    /// <summary>Tamaño máximo permitido por archivo: 10 MB (RF-DOC-01).</summary>
    public const long TamanoMaximoBytes = 10 * 1024 * 1024;

    private const int LongitudMaximaNombre = 255;
    private const int LongitudMaximaTipoContenido = 100;

    public async Task<DocumentoResumen> SubirAsync(
        int usuarioId,
        string nombre,
        string tipoContenido,
        Stream contenido,
        long tamano,
        CancellationToken cancellationToken = default)
    {
        ValidarSubida(nombre, tipoContenido, contenido, tamano, out var nombreLimpio, out var tipoLimpio);

        // RF-DOC-01: el nombre físico es un Guid; nunca el nombre del usuario.
        var nombreFisico = Guid.NewGuid().ToString("N");
        var ruta = await almacenamiento.GuardarAsync(contenido, nombreFisico, cancellationToken);

        try
        {
            var documento = new Documento
            {
                Nombre = nombreLimpio,
                TipoContenido = tipoLimpio,
                TamanoBytes = tamano,
                FechaSubida = timeProvider.GetUtcNow().UtcDateTime,
                UsuarioId = usuarioId,
                RutaAlmacenamiento = ruta,
                Eliminado = false,
            };

            await unidad.Documentos.AgregarAsync(documento, cancellationToken);
            await unidad.GuardarCambiosAsync(cancellationToken);

            return AResumen(documento);
        }
        catch
        {
            // RF-DOC-01: si el registro no se confirma, no debe quedar el archivo huérfano.
            await almacenamiento.EliminarAsync(ruta, CancellationToken.None);
            throw;
        }
    }

    public async Task<IReadOnlyList<DocumentoResumen>> ListarAsync(
        int usuarioId,
        RolUsuario rol,
        CancellationToken cancellationToken = default)
    {
        var documentos = rol == RolUsuario.Administrador
            ? await unidad.Documentos.ListarTodosAsync(cancellationToken)
            : await unidad.Documentos.ListarPorUsuarioAsync(usuarioId, cancellationToken);

        // RF-DOC-02: nunca se listan documentos con borrado lógico.
        return documentos
            .Where(d => !d.Eliminado)
            .Select(AResumen)
            .ToList();
    }

    public async Task<DocumentoDescarga> DescargarAsync(
        int usuarioId,
        RolUsuario rol,
        int id,
        CancellationToken cancellationToken = default)
    {
        var documento = await ObtenerVisibleAsync(usuarioId, rol, id, cancellationToken);

        Stream contenido;
        try
        {
            contenido = await almacenamiento.AbrirAsync(documento.RutaAlmacenamiento, cancellationToken);
        }
        catch (FileNotFoundException)
        {
            // El registro existe, pero el archivo físico ya no está.
            throw new ReglaNegocioExcepcion("El documento no existe.", 404);
        }

        return new DocumentoDescarga(contenido, documento.Nombre, documento.TipoContenido);
    }

    public async Task EliminarAsync(
        int usuarioId,
        RolUsuario rol,
        int id,
        CancellationToken cancellationToken = default)
    {
        var documento = await ObtenerVisibleAsync(usuarioId, rol, id, cancellationToken);

        // RF-DOC-04: borrado lógico. El registro y el archivo físico se conservan.
        documento.Eliminado = true;
        await unidad.GuardarCambiosAsync(cancellationToken);
    }

    /// <summary>
    /// Devuelve el documento solo si existe, no está eliminado y el solicitante es
    /// su dueño o un Administrador (RF-DOC-03, RF-DOC-04).
    /// </summary>
    private async Task<Documento> ObtenerVisibleAsync(
        int usuarioId,
        RolUsuario rol,
        int id,
        CancellationToken cancellationToken)
    {
        var documento = await unidad.Documentos.BuscarPorIdAsync(id, cancellationToken);

        if (documento is null || documento.Eliminado)
        {
            throw new ReglaNegocioExcepcion("El documento no existe.", 404);
        }

        if (rol != RolUsuario.Administrador && documento.UsuarioId != usuarioId)
        {
            throw new ReglaNegocioExcepcion("No tienes permiso para acceder a este documento.", 403);
        }

        return documento;
    }

    private static void ValidarSubida(
        string nombre,
        string tipoContenido,
        Stream contenido,
        long tamano,
        out string nombreLimpio,
        out string tipoLimpio)
    {
        // RD-07: archivo ausente o vacío.
        if (contenido is null)
        {
            throw new ReglaNegocioExcepcion("El archivo es obligatorio.");
        }

        // RD-07: el nombre se limpia para descartar cualquier ruta que envíe el cliente.
        nombreLimpio = Path.GetFileName((nombre ?? string.Empty).Trim());

        if (string.IsNullOrWhiteSpace(nombreLimpio) || nombreLimpio is "." or "..")
        {
            throw new ReglaNegocioExcepcion("El nombre del archivo es obligatorio.");
        }

        if (nombreLimpio.Length > LongitudMaximaNombre)
        {
            throw new ReglaNegocioExcepcion(
                $"El nombre del archivo no puede superar los {LongitudMaximaNombre} caracteres.");
        }

        if (tamano <= 0)
        {
            throw new ReglaNegocioExcepcion("El archivo está vacío.");
        }

        if (tamano > TamanoMaximoBytes)
        {
            throw new ReglaNegocioExcepcion(
                $"El archivo no puede superar los {TamanoMaximoBytes / (1024 * 1024)} MB.");
        }

        tipoLimpio = string.IsNullOrWhiteSpace(tipoContenido)
            ? "application/octet-stream"
            : tipoContenido.Trim();

        if (tipoLimpio.Length > LongitudMaximaTipoContenido)
        {
            throw new ReglaNegocioExcepcion(
                $"El tipo de contenido no puede superar los {LongitudMaximaTipoContenido} caracteres.");
        }
    }

    private static DocumentoResumen AResumen(Documento documento)
        => new(
            documento.Id,
            documento.Nombre,
            documento.TipoContenido,
            documento.TamanoBytes,
            documento.FechaSubida,
            documento.UsuarioId);
}
