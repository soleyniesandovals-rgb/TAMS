using System.Reflection;
using Core.Auditoria.Application.Interfaces;
using Core.Auditoria.Application.Modelos;
using Core.Auditoria.Domain.Entidades;
using Core.Auditoria.Domain.Enums;
using Core.Auditoria.Infrastructura.Repositorios;
using Core.ControlAcceso.Infrastructura.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Tests.Core;

/// <summary>
/// RF-AUD-07 (CA-14, CA-15, CA-16): la consulta del repositorio filtra en la base,
/// ordena de más reciente a más antiguo, limita a 500 filas y nunca modifica el
/// historial. Sin levantar la aplicación ni usar SQL Server (RD-12).
/// </summary>
public class ConsultaAuditoriaRepositorioTests
{
    [Fact]
    public async Task RF_AUD_07_SinFiltros_DevuelveTodoConTodosLosCamposYOrdenadoPorFechaYId()
    {
        // Preparar: registros de varios usuarios, entidades y fechas, con un desempate por Id.
        await using var contexto = CrearContexto();
        contexto.RegistrosAuditoria.AddRange(
            Registro(7, AccionAuditoria.SubirDocumento, EntidadAuditoria.Documento, 101,
                new DateTime(2026, 11, 20, 8, 0, 0, DateTimeKind.Utc), null, "a.pdf (pdf, 10 bytes)"),
            Registro(3, AccionAuditoria.AprobarSolicitud, EntidadAuditoria.SolicitudPermiso, 55,
                new DateTime(2026, 11, 21, 23, 59, 0, DateTimeKind.Utc), "Pendiente", "Aplicada (Vacaciones)"),
            Registro(3, AccionAuditoria.CambiarRol, EntidadAuditoria.Usuario, 9,
                new DateTime(2026, 11, 21, 23, 59, 0, DateTimeKind.Utc), "Estándar", "Administrador"));
        await contexto.SaveChangesAsync();
        var repositorio = new RegistrosAuditoriaRepositorio(contexto);

        // Actuar: se consulta sin ningún filtro.
        var registros = await repositorio.ConsultarAsync(SinFiltros(), CancellationToken.None);

        // Verificar: CA-15 se devuelven todos los campos, ordenados por FechaHora y luego Id descendentes.
        Assert.Equal([3, 2, 1], registros.Select(r => r.Id).ToList());
        var reciente = registros[0];
        Assert.Equal(3, reciente.Id);
        Assert.Equal(3, reciente.UsuarioId);
        Assert.Equal(AccionAuditoria.CambiarRol, reciente.Accion);
        Assert.Equal(EntidadAuditoria.Usuario, reciente.Entidad);
        Assert.Equal(9, reciente.EntidadId);
        Assert.Equal(new DateTime(2026, 11, 21, 23, 59, 0, DateTimeKind.Utc), reciente.FechaHora);
        Assert.Equal("Estándar", reciente.ValorAnterior);
        Assert.Equal("Administrador", reciente.ValorNuevo);
    }

    [Fact]
    public async Task RF_AUD_07_MasDe500Registros_DevuelveExactamente500()
    {
        // Preparar: 501 registros con fechas crecientes.
        await using var contexto = CrearContexto();
        for (var i = 1; i <= 501; i++)
        {
            contexto.RegistrosAuditoria.Add(Registro(1, AccionAuditoria.SubirDocumento,
                EntidadAuditoria.Documento, i,
                new DateTime(2026, 11, 1, 0, 0, 0, DateTimeKind.Utc).AddMinutes(i), null, $"Documento {i}"));
        }

        await contexto.SaveChangesAsync();
        var repositorio = new RegistrosAuditoriaRepositorio(contexto);

        // Actuar: se consulta sin filtros.
        var registros = await repositorio.ConsultarAsync(SinFiltros(), CancellationToken.None);

        // Verificar: CA-15 el tope de 500 se aplica en la base y se conserva el orden descendente.
        Assert.Equal(FiltrosConsultaAuditoria.MaximoRegistros, registros.Count);
        Assert.Equal(501, registros[0].Id);
        Assert.DoesNotContain(registros, r => r.Id == 1);
    }

    [Fact]
    public async Task RF_AUD_07_FiltrosCombinados_SeAplicanEnLaConsulta()
    {
        // Preparar: cuatro registros que difieren en usuario, entidad y fecha.
        await using var contexto = CrearContexto();
        contexto.RegistrosAuditoria.AddRange(
            Registro(3, AccionAuditoria.SubirDocumento, EntidadAuditoria.Documento, 1,
                new DateTime(2026, 11, 20, 10, 0, 0, DateTimeKind.Utc), null, "Antes del rango"),
            Registro(3, AccionAuditoria.SubirDocumento, EntidadAuditoria.Documento, 2,
                new DateTime(2026, 11, 21, 23, 59, 0, DateTimeKind.Utc), null, "Dentro del rango"),
            Registro(3, AccionAuditoria.CambiarRol, EntidadAuditoria.Usuario, 3,
                new DateTime(2026, 11, 21, 12, 0, 0, DateTimeKind.Utc), "Estándar", "Administrador"),
            Registro(8, AccionAuditoria.SubirDocumento, EntidadAuditoria.Documento, 4,
                new DateTime(2026, 11, 21, 23, 59, 0, DateTimeKind.Utc), null, "Otro usuario"));
        await contexto.SaveChangesAsync();
        var repositorio = new RegistrosAuditoriaRepositorio(contexto);

        // Actuar: se combinan los cuatro filtros ya validados.
        var registros = await repositorio.ConsultarAsync(
            new FiltrosConsultaAuditoria(
                3,
                EntidadAuditoria.Documento,
                new DateTime(2026, 11, 21, 0, 0, 0, DateTimeKind.Utc),
                new DateTime(2026, 11, 21, 23, 59, 59, DateTimeKind.Utc).AddTicks(9999999)),
            CancellationToken.None);

        // Verificar: CA-16 solo sobrevive el registro que cumple los cuatro filtros.
        var registro = Assert.Single(registros);
        Assert.Equal(2, registro.Id);
    }

    [Fact]
    public void RF_AUD_07_Repositorio_SoloAgregaYConsulta_SinEditarNiBorrar()
    {
        // Preparar: la interfaz, su implementación y los verbos prohibidos (CA-14).
        var verbosProhibidos = new[]
        {
            "Actualizar", "Editar", "Modificar", "Borrar", "Eliminar", "Update", "Delete", "Remove",
        };

        // Actuar: se listan los métodos públicos declarados en ambos tipos.
        var metodos = typeof(IRegistrosAuditoriaRepositorio).GetMethods()
            .Concat(typeof(RegistrosAuditoriaRepositorio).GetMethods(
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            .Select(m => m.Name)
            .Distinct()
            .ToList();

        // Verificar: CA-14 la única superficie es agregar y consultar; no hay escritura ni borrado.
        Assert.Equal(new[] { "AgregarAsync", "ConsultarAsync" }, metodos);
        Assert.DoesNotContain(metodos, nombre =>
            verbosProhibidos.Any(verbo => nombre.Contains(verbo, StringComparison.OrdinalIgnoreCase)));
    }

    [Fact]
    public async Task RF_AUD_07_RegistroExistente_TrasAgregarOtro_SigueIgual()
    {
        // Preparar: un registro ya guardado y todos sus valores.
        await using var contexto = CrearContexto();
        var repositorio = new RegistrosAuditoriaRepositorio(contexto);
        await repositorio.AgregarAsync(Registro(4, AccionAuditoria.DesactivarUsuario,
            EntidadAuditoria.Usuario, 12,
            new DateTime(2026, 11, 20, 9, 0, 0, DateTimeKind.Utc), "Activo", "Inactivo"));
        await contexto.SaveChangesAsync();

        // Actuar: se agrega y guarda un registro posterior.
        await repositorio.AgregarAsync(Registro(5, AccionAuditoria.ReactivarUsuario,
            EntidadAuditoria.Usuario, 12,
            new DateTime(2026, 11, 21, 9, 0, 0, DateTimeKind.Utc), "Inactivo", "Activo"));
        await contexto.SaveChangesAsync();
        var original = await contexto.RegistrosAuditoria.AsNoTracking().SingleAsync(r => r.Id == 1);

        // Verificar: CA-14 el registro anterior no se ha tocado.
        Assert.Equal(4, original.UsuarioId);
        Assert.Equal(AccionAuditoria.DesactivarUsuario, original.Accion);
        Assert.Equal(EntidadAuditoria.Usuario, original.Entidad);
        Assert.Equal(12, original.EntidadId);
        Assert.Equal(new DateTime(2026, 11, 20, 9, 0, 0, DateTimeKind.Utc), original.FechaHora);
        Assert.Equal("Activo", original.ValorAnterior);
        Assert.Equal("Inactivo", original.ValorNuevo);
    }

    private static FiltrosConsultaAuditoria SinFiltros() => new(null, null, null, null);

    private static CoreDbContext CrearContexto()
    {
        var opciones = new DbContextOptionsBuilder<CoreDbContext>()
            .UseInMemoryDatabase($"auditoria-consulta-repositorio-{Guid.NewGuid():N}")
            .Options;
        return new CoreDbContext(opciones, TimeProvider.System);
    }

    private static RegistroAuditoria Registro(
        int usuarioId,
        AccionAuditoria accion,
        EntidadAuditoria entidad,
        int entidadId,
        DateTime fechaHora,
        string? valorAnterior,
        string? valorNuevo)
        => RegistroAuditoria.Crear(usuarioId, accion, entidad, entidadId, fechaHora, valorAnterior, valorNuevo);
}
