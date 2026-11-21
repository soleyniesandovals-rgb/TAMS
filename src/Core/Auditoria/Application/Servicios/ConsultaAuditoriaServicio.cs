using System.Globalization;
using Core.Auditoria.Application.Interfaces;
using Core.Auditoria.Application.Modelos;
using Core.Auditoria.Domain.Enums;
using Core.ControlAcceso.Application.Interfaces;
using Core.ControlAcceso.Domain.Excepciones;

namespace Core.Auditoria.Application.Servicios;

/// <summary>
/// Valida los filtros recibidos como texto y consulta el historial (RF-AUD-07, CA-16 a CA-18,
/// RD-07, RD-02). Solo lee: no expone editar ni borrar (CA-14). Proyecta la fecha como UTC
/// para que se serialice con <c>Z</c> (RD-11).
/// </summary>
public class ConsultaAuditoriaServicio(ICoreUnidadDeTrabajo unidad) : IConsultaAuditoriaServicio
{
    private static readonly string[] EntidadesPermitidas = Enum.GetNames<EntidadAuditoria>();

    public async Task<IReadOnlyList<RegistroAuditoriaResumen>> ListarAsync(
        string? usuarioId,
        string? entidad,
        string? desde,
        string? hasta,
        CancellationToken cancellationToken = default)
    {
        // RD-07/CA-18: nada se convierte directamente en enum ni en fecha sin validar antes.
        var desdeFecha = ParsearFecha(desde, nameof(desde));
        var hastaFecha = ParsearFecha(hasta, nameof(hasta));

        if (desdeFecha > hastaFecha)
        {
            throw new ReglaNegocioExcepcion("desde no puede ser posterior a hasta.");
        }

        var filtros = new FiltrosConsultaAuditoria(
            ParsearUsuarioId(usuarioId),
            ParsearEntidad(entidad),
            // Un extremo ausente significa «sin filtro».
            desdeFecha?.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
            // CA-16: el último instante del día incluye cualquier hora de «hasta». Con
            // 9999-12-31 esto es DateTime.MaxValue, que no provoca desbordamiento (A3-H1).
            hastaFecha?.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc));

        var registros = await unidad.RegistrosAuditoria.ConsultarAsync(filtros, cancellationToken);

        // RD-11: el almacenamiento puede devolver la fecha sin marca de zona; se marca UTC.
        return registros
            .Select(r => new RegistroAuditoriaResumen(
                r.Id,
                r.UsuarioId,
                r.Accion,
                r.Entidad,
                r.EntidadId,
                DateTime.SpecifyKind(r.FechaHora, DateTimeKind.Utc),
                r.ValorAnterior,
                r.ValorNuevo))
            .ToList();
    }

    private static int? ParsearUsuarioId(string? valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            return null;
        }

        // CA-18: "0", "-1" y "abc" son inválidos; solo se admite un entero positivo.
        if (!int.TryParse(valor.Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out var id)
            || id <= 0)
        {
            throw new ReglaNegocioExcepcion("usuarioId debe ser un entero positivo.");
        }

        return id;
    }

    private static EntidadAuditoria? ParsearEntidad(string? valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            return null;
        }

        // A3-H2/RD-07: Enum.TryParse acepta números ("0") y combinaciones con comas
        // ("Usuario,Documento"). Solo se admiten los NOMBRES exactos, sin distinguir mayúsculas.
        var texto = valor.Trim();
        var nombre = Array.Find(EntidadesPermitidas,
            n => string.Equals(n, texto, StringComparison.OrdinalIgnoreCase));

        if (nombre is null)
        {
            throw new ReglaNegocioExcepcion(
                "La entidad indicada no es válida. Valores permitidos: SolicitudPermiso, Documento, Usuario.");
        }

        return Enum.Parse<EntidadAuditoria>(nombre, ignoreCase: false);
    }

    private static DateOnly? ParsearFecha(string? valor, string nombre)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            return null;
        }

        if (!DateOnly.TryParseExact(
                valor.Trim(),
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var fecha))
        {
            throw new ReglaNegocioExcepcion($"{nombre} debe tener el formato AAAA-MM-DD.");
        }

        return fecha;
    }
}
