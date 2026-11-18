namespace Tests.Core.Dobles;

/// <summary>Reloj mutable para probar RD-11 sin depender de la hora real.</summary>
public sealed class TimeProviderFijo(DateTimeOffset instante) : TimeProvider
{
    public DateTimeOffset Instante { get; set; } = instante;

    public override DateTimeOffset GetUtcNow() => Instante;
}
