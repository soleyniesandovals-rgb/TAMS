using System.Reflection;
using Core.Auditoria.Application.Interfaces;
using Core.Auditoria.Domain.Entidades;
using Core.Auditoria.Infrastructura.Repositorios;
using Xunit;

namespace Tests.Core;

/// <summary>
/// CA-14 (RF-AUD-06): el historial no se puede modificar ni borrar desde la aplicación.
/// </summary>
public class InmutabilidadAuditoriaTests
{
    [Fact]
    public void RF_AUD_06_RegistroAuditoria_NoExponeSettersPublicosNiConstructoresPublicos()
    {
        // CA-14: la entidad solo se crea con su fábrica y no cambia después de creada.
        // Preparar: se inspecciona el tipo del registro por reflexión.
        var tipo = typeof(RegistroAuditoria);

        // Actuar: se listan sus constructores y propiedades públicas.
        var constructoresPublicos = tipo.GetConstructors();
        var propiedades = tipo.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var settersPublicos = propiedades
            .Where(p => p.SetMethod is { IsPublic: true })
            .Select(p => p.Name)
            .ToList();

        // Verificar: no se puede construir ni modificar desde fuera del dominio.
        Assert.Empty(constructoresPublicos);
        Assert.Empty(settersPublicos);
        Assert.NotNull(tipo.GetMethod("Crear", BindingFlags.Public | BindingFlags.Static));
    }

    [Fact]
    public void RF_AUD_06_RepositorioAuditoria_SoloExponeAgregarSinEditarNiBorrar()
    {
        // CA-14: el repositorio del historial solo agrega; no hay editar ni borrar.
        // Preparar: la interfaz, su implementación y los verbos prohibidos.
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

        // Verificar: el único método es AgregarAsync y no aparece ningún verbo de cambio o borrado.
        Assert.Equal(new[] { "AgregarAsync" }, metodos);
        Assert.DoesNotContain(metodos, nombre =>
            verbosProhibidos.Any(verbo => nombre.Contains(verbo, StringComparison.OrdinalIgnoreCase)));
    }
}
