namespace Core.ControlAcceso.Domain.Interfaces;

/// <summary>
/// Contrato para las fechas de creación. El <c>CoreDbContext</c> lo sella en UTC
/// con el <see cref="System.TimeProvider"/> inyectado (RD-11, RD-12).
/// </summary>
public interface ICreacionAuditable
{
    DateTime FechaCreacion { get; set; }
}