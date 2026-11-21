using Core.Auditoria.Application.Interfaces;
using Core.Auditoria.Application.Servicios;
using Core.Auditoria.Domain.Entidades;
using Core.Auditoria.Domain.Enums;
using Core.ControlAcceso.Application.Interfaces;
using Core.ControlAcceso.Domain.Entidades;
using Core.ControlAcceso.Domain.Enums;
using Core.ControlAcceso.Domain.Excepciones;
using Core.ControlAcceso.Infrastructura.Persistence;
using Core.ControlAcceso.Infrastructura.Repositorios;
using Core.Documentos.Application.Interfaces;
using Core.Documentos.Application.Servicios;
using Core.Documentos.Domain.Entidades;
using Core.Notificaciones.Application.Interfaces;
using Core.Permisos.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Tests.Core.Dobles;
using Xunit;

namespace Tests.Core;

/// <summary>
/// RF-AUD-03: auditoría de documentos (CA-04, CA-05, CA-12 y CA-13). Se usan los
/// repositorios reales sobre EF InMemory para obtener el id generado por la base y
/// comprobar la transacción única de la subida, sin levantar la aplicación ni SQL Server.
/// </summary>
public class DocumentosAuditoriaTests
{
    [Fact]
    public async Task RF_AUD_03_SubirAsync_RegistraLosSeisCamposConElIdGenerado()
    {
        // CA-04: SubirDocumento / Documento / id de la base / null -> nombre (tipo, bytes).
        // Preparar: un escenario con almacenamiento en memoria y reloj fijo.
        using var escenario = new Escenario();

        // Actuar: se sube un archivo de tres bytes.
        var resumen = await escenario.Servicio.SubirAsync(
            7, "guia.pdf", "application/pdf", new MemoryStream([1, 2, 3]), 3, CancellationToken.None);

        // Verificar: el id del registro es el que generó la base al guardar el documento.
        Assert.True(resumen.Id > 0);
        var registro = Assert.Single(escenario.Registros);
        Assert.Equal(7, registro.UsuarioId);
        Assert.Equal(AccionAuditoria.SubirDocumento, registro.Accion);
        Assert.Equal(EntidadAuditoria.Documento, registro.Entidad);
        Assert.Equal(resumen.Id, registro.EntidadId);
        Assert.Null(registro.ValorAnterior);
        Assert.Equal("guia.pdf (application/pdf, 3 bytes)", registro.ValorNuevo);
    }

    [Fact]
    public async Task RF_AUD_03_EliminarAsync_Dueno_RegistraLosSeisCampos()
    {
        // CA-05: EliminarDocumento con el dueño como actor y Activo -> Eliminado.
        // Preparar: un documento activo propiedad del usuario 7.
        using var escenario = new Escenario();
        await escenario.SembrarDocumentoAsync(11, 7);

        // Actuar: el dueño lo elimina.
        await escenario.Servicio.EliminarAsync(7, RolUsuario.Estandar, 11, CancellationToken.None);

        // Verificar: un solo registro con los seis campos exigidos.
        var registro = Assert.Single(escenario.Registros);
        Assert.Equal(7, registro.UsuarioId);
        Assert.Equal(AccionAuditoria.EliminarDocumento, registro.Accion);
        Assert.Equal(EntidadAuditoria.Documento, registro.Entidad);
        Assert.Equal(11, registro.EntidadId);
        Assert.Equal("Activo", registro.ValorAnterior);
        Assert.Equal("Eliminado", registro.ValorNuevo);
    }

    [Fact]
    public async Task RF_AUD_03_EliminarAsync_Administrador_RegistraAlActor()
    {
        // CA-05: el actor es quien elimina, aunque no sea el dueño del documento.
        // Preparar: un documento del usuario 7 y un administrador 9.
        using var escenario = new Escenario();
        await escenario.SembrarDocumentoAsync(12, 7);

        // Actuar: el administrador elimina el documento ajeno.
        await escenario.Servicio.EliminarAsync(9, RolUsuario.Administrador, 12, CancellationToken.None);

        // Verificar: el usuarioId del registro es el del administrador, no el del dueño.
        var registro = Assert.Single(escenario.Registros);
        Assert.Equal(9, registro.UsuarioId);
        Assert.Equal(AccionAuditoria.EliminarDocumento, registro.Accion);
        Assert.Equal(EntidadAuditoria.Documento, registro.Entidad);
        Assert.Equal(12, registro.EntidadId);
        Assert.Equal("Activo", registro.ValorAnterior);
        Assert.Equal("Eliminado", registro.ValorNuevo);
    }

    [Fact]
    public async Task RF_AUD_03_SubirAsync_DocumentoYRegistroEnUnaSolaTransaccion()
    {
        // CA-12: una transacción, los dos guardados dentro y un solo commit.
        // Preparar: unidad real sobre InMemory que cuenta las transacciones y los guardados.
        using var escenario = new Escenario();

        // Actuar: una subida correcta.
        await escenario.Servicio.SubirAsync(
            7, "guia.pdf", "application/pdf", new MemoryStream([1, 2, 3, 4]), 4, CancellationToken.None);

        // Verificar: el documento se guardó para obtener el id y el registro se guardó
        // después, ambos dentro de la misma transacción confirmada al final.
        Assert.Equal(1, escenario.Unidad.Transacciones);
        Assert.Equal(1, escenario.Unidad.Confirmadas);
        Assert.Equal(0, escenario.Unidad.Revertidas);
        Assert.Equal(2, escenario.Unidad.Guardados);
        Assert.Equal(2, escenario.Unidad.GuardadosEnTransaccion);
        Assert.Single(escenario.Registros);
    }

    [Fact]
    public async Task RF_AUD_03_SubirAsync_FalloDelRegistro_RevierteYLimpiaElArchivo()
    {
        // CA-12: si falla el registro, se revierte la transacción y no queda el archivo.
        // Preparar: un registrador que falla después de haberse guardado el documento.
        using var escenario = new Escenario(new RegistradorQueFalla());

        // Actuar: la subida debe propagar el fallo del registro.
        await Assert.ThrowsAsync<InvalidOperationException>(() => escenario.Servicio.SubirAsync(
            7, "guia.pdf", "application/pdf", new MemoryStream([1, 2, 3]), 3, CancellationToken.None));

        // Verificar: se revirtió la transacción, no hubo commit y se limpió el archivo.
        Assert.Equal(1, escenario.Unidad.Transacciones);
        Assert.Equal(0, escenario.Unidad.Confirmadas);
        Assert.Equal(1, escenario.Unidad.Revertidas);
        Assert.Empty(escenario.Registros);
        Assert.Single(escenario.Almacenamiento.RutasEliminadas);
    }

    [Fact]
    public async Task RF_AUD_03_EliminarAsync_EstandarDeDocumentoAjeno_NoRegistra()
    {
        // CA-13: un rechazo (403) no crea ningún registro.
        // Preparar: un documento del usuario 7 intentado eliminar por el usuario 8.
        using var escenario = new Escenario();
        await escenario.SembrarDocumentoAsync(13, 7);

        // Actuar: un Estándar intenta eliminar un documento ajeno.
        await Assert.ThrowsAsync<ReglaNegocioExcepcion>(() => escenario.Servicio.EliminarAsync(
            8, RolUsuario.Estandar, 13, CancellationToken.None));

        // Verificar: ni registro ni guardado, y el documento sigue activo.
        Assert.Empty(escenario.Registros);
        Assert.Equal(0, escenario.Unidad.Guardados);
        var documento = await escenario.Contexto.Documentos.AsNoTracking().SingleAsync(d => d.Id == 13);
        Assert.False(documento.Eliminado);
    }

    /// <summary>Escenario nuevo por prueba, con base InMemory única (RD-12).</summary>
    private sealed class Escenario : IDisposable
    {
        private static readonly TimeProvider RelojFijo =
            new TimeProviderFijo(new DateTimeOffset(2026, 11, 21, 12, 0, 0, TimeSpan.Zero));

        private readonly CoreDbContext _contexto;

        public Escenario(IRegistradorAuditoria? registrador = null)
        {
            var opciones = new DbContextOptionsBuilder<CoreDbContext>()
                .UseInMemoryDatabase($"tams-auditoria-documentos-{Guid.NewGuid():N}")
                .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            _contexto = new CoreDbContext(opciones, RelojFijo);
            _contexto.Usuarios.AddRange(
                new Usuario
                {
                    Id = 7,
                    Nombre = "Dueño",
                    Correo = "dueno@tams.test",
                    ContraseñaHash = "hash-de-prueba",
                    Rol = RolUsuario.Estandar,
                    Activo = true,
                },
                new Usuario
                {
                    Id = 8,
                    Nombre = "Otro",
                    Correo = "otro@tams.test",
                    ContraseñaHash = "hash-de-prueba",
                    Rol = RolUsuario.Estandar,
                    Activo = true,
                },
                new Usuario
                {
                    Id = 9,
                    Nombre = "Administrador",
                    Correo = "admin@tams.test",
                    ContraseñaHash = "hash-de-prueba",
                    Rol = RolUsuario.Administrador,
                    Activo = true,
                });
            _contexto.SaveChanges();

            Unidad = new UnidadEspia(new CoreUnidadDeTrabajo(_contexto));
            Registrador = registrador ?? new RegistradorAuditoria(Unidad, RelojFijo);
            Servicio = new GestionDocumentosServicio(Unidad, Registrador, Almacenamiento, RelojFijo);
        }

        public CoreDbContext Contexto => _contexto;

        public UnidadEspia Unidad { get; private set; } = null!;

        public IRegistradorAuditoria Registrador { get; private set; } = null!;

        public GestionDocumentosServicio Servicio { get; private set; } = null!;

        public AlmacenamientoFalso Almacenamiento { get; } = new();

        public IReadOnlyList<RegistroAuditoria> Registros
            => _contexto.RegistrosAuditoria.AsNoTracking().ToList();

        public async Task SembrarDocumentoAsync(int id, int usuarioId)
        {
            _contexto.Documentos.Add(new Documento
            {
                Id = id,
                Nombre = "guia.pdf",
                TipoContenido = "application/pdf",
                TamanoBytes = 12,
                FechaSubida = RelojFijo.GetUtcNow().UtcDateTime,
                UsuarioId = usuarioId,
                RutaAlmacenamiento = "archivos/guia.pdf",
                Eliminado = false,
            });
            await _contexto.SaveChangesAsync();
        }

        public void Dispose()
        {
            Unidad.Dispose();
        }
    }

    /// <summary>Unidad real con contadores de transacción y guardado (CA-12).</summary>
    private sealed class UnidadEspia(CoreUnidadDeTrabajo inner) : ICoreUnidadDeTrabajo
    {
        private bool _enTransaccion;

        public int Transacciones { get; private set; }
        public int Confirmadas { get; private set; }
        public int Revertidas { get; private set; }
        public int Guardados { get; private set; }
        public int GuardadosEnTransaccion { get; private set; }

        public IUsuariosRepositorio Usuarios => inner.Usuarios;
        public ITokenActivacionesRepositorio TokenActivaciones => inner.TokenActivaciones;
        public ICorreosEnColaRepositorio CorreosEnCola => inner.CorreosEnCola;
        public ICodigosRecuperacionRepositorio CodigosRecuperacion => inner.CodigosRecuperacion;
        public ISolicitudesPermisoRepositorio SolicitudesPermiso => inner.SolicitudesPermiso;
        public IPermisosActivosRepositorio PermisosActivos => inner.PermisosActivos;
        public IDocumentosRepositorio Documentos => inner.Documentos;
        public INotificacionesRepositorio Notificaciones => inner.Notificaciones;
        public IPreferenciasNotificacionRepositorio PreferenciasNotificacion => inner.PreferenciasNotificacion;
        public IRegistrosAuditoriaRepositorio RegistrosAuditoria => inner.RegistrosAuditoria;

        public Task<int> GuardarCambiosAsync(CancellationToken cancellationToken = default)
        {
            Guardados++;
            if (_enTransaccion)
            {
                GuardadosEnTransaccion++;
            }

            return inner.GuardarCambiosAsync(cancellationToken);
        }

        public async Task<T> EjecutarEnTransaccionAsync<T>(
            Func<CancellationToken, Task<T>> operacion,
            CancellationToken cancellationToken = default)
        {
            Transacciones++;
            _enTransaccion = true;
            try
            {
                var resultado = await inner.EjecutarEnTransaccionAsync(operacion, cancellationToken);
                Confirmadas++;
                return resultado;
            }
            catch
            {
                Revertidas++;
                throw;
            }
            finally
            {
                _enTransaccion = false;
            }
        }

        public void Dispose() => inner.Dispose();
    }

    private sealed class AlmacenamientoFalso : IAlmacenamientoArchivos
    {
        public List<string> RutasEliminadas { get; } = [];

        public Task<string> GuardarAsync(Stream contenido, string nombreFisico, CancellationToken cancellationToken = default)
            => Task.FromResult($"archivos/{nombreFisico}");

        public Task<Stream> AbrirAsync(string ruta, CancellationToken cancellationToken = default)
            => Task.FromResult<Stream>(new MemoryStream());

        public Task EliminarAsync(string ruta, CancellationToken cancellationToken = default)
        {
            RutasEliminadas.Add(ruta);
            return Task.CompletedTask;
        }
    }

    private sealed class RegistradorQueFalla : IRegistradorAuditoria
    {
        public Task RegistrarAsync(
            int usuarioId,
            AccionAuditoria accion,
            EntidadAuditoria entidad,
            int entidadId,
            string? valorAnterior,
            string? valorNuevo,
            CancellationToken cancellationToken = default)
            => throw new InvalidOperationException("Fallo simulado al registrar la auditoría.");
    }
}
