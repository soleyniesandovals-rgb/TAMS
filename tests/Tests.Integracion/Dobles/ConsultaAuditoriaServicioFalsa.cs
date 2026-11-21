using Core.Auditoria.Application.Interfaces;
using Core.Auditoria.Application.Modelos;

namespace Tests.Integracion.Dobles;

/// <summary>
/// Doble en memoria del servicio de consulta para probar el controlador sin levantar
/// la aplicación ni usar SQL Server (RD-12).
/// </summary>
internal sealed class ConsultaAuditoriaServicioFalsa : IConsultaAuditoriaServicio
{
    public IReadOnlyList<RegistroAuditoriaResumen> Resultado { get; set; } = [];

    public Exception? Excepcion { get; set; }

    public string? UsuarioIdRecibido { get; private set; }

    public string? EntidadRecibida { get; private set; }

    public string? DesdeRecibido { get; private set; }

    public string? HastaRecibido { get; private set; }

    public Task<IReadOnlyList<RegistroAuditoriaResumen>> ListarAsync(
        string? usuarioId,
        string? entidad,
        string? desde,
        string? hasta,
        CancellationToken cancellationToken = default)
    {
        UsuarioIdRecibido = usuarioId;
        EntidadRecibida = entidad;
        DesdeRecibido = desde;
        HastaRecibido = hasta;

        if (Excepcion is not null)
        {
            throw Excepcion;
        }

        return Task.FromResult(Resultado);
    }
}
