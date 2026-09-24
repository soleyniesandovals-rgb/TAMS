namespace Tams.Negocio.Domain.Interfaces;

/// <summary>
/// RD-11: las entidades que registran su fecha de creación la almacenan en UTC.
/// El valor se asigna en <c>TamsDbContext</c> al insertar y nunca se usa <c>DateTime.Now</c> (hora local).
/// </summary>
public interface ICreacionAuditable
{
    DateTime FechaCreacion { get; set; }
}