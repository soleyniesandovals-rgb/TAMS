using Core.ControlAcceso.Application.Interfaces;
using Core.ControlAcceso.Application.Modelos;
using Core.ControlAcceso.Domain.Enums;
using Core.ControlAcceso.Domain.Excepciones;

namespace Core.ControlAcceso.Application.Servicios;

/// <summary>
/// Consulta de la cola de correos (RF-NOT-11, RD-02, RD-11): valida el filtro de
/// estado y proyecta los correos sin exponer su cuerpo.
/// </summary>
public class ConsultaCorreosEnColaServicio(ICoreUnidadDeTrabajo unidad) : IConsultaCorreosEnColaServicio
{
    public async Task<IReadOnlyList<CorreoEnColaResumen>> ListarAsync(
        string? estado,
        CancellationToken cancellationToken = default)
    {
        EstadoCorreo? filtro = null;

        if (!string.IsNullOrWhiteSpace(estado))
        {
            // RF-NOT-11/RD-07: Enum.TryParse acepta números ("0", "1") y combinaciones
            // separadas por comas ("Pendiente,Enviado"). Solo se admiten los NOMBRES del
            // estado; el filtro ausente o vacío sigue significando «sin filtro».
            var texto = estado.Trim();
            if (!Enum.TryParse<EstadoCorreo>(texto, ignoreCase: true, out var estadoValido)
                || !Enum.IsDefined(estadoValido)
                || !string.Equals(estadoValido.ToString(), texto, StringComparison.OrdinalIgnoreCase))
            {
                throw new ReglaNegocioExcepcion(
                    "El estado indicado no es válido. Valores permitidos: Pendiente, Enviando, Enviado, Fallido.");
            }

            filtro = estadoValido;
        }

        var correos = await unidad.CorreosEnCola.ListarAsync(filtro, cancellationToken);

        return correos
            .Select(c => new CorreoEnColaResumen(
                c.Id,
                c.Destinatario,
                c.Asunto,
                c.Estado,
                c.Intentos,
                DateTime.SpecifyKind(c.FechaCreacion, DateTimeKind.Utc),
                c.FechaUltimoIntento is null ? null : DateTime.SpecifyKind(c.FechaUltimoIntento.Value, DateTimeKind.Utc),
                c.FechaEnvio is null ? null : DateTime.SpecifyKind(c.FechaEnvio.Value, DateTimeKind.Utc),
                c.UltimoError))
            .ToList();
    }
}
