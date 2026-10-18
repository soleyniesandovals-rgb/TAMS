public class TimeProviderFijo : TimeProvider
{
    private readonly DateTimeOffset _utcNow;

    public TimeProviderFijo(DateTimeOffset utcNow)
    {
        _utcNow = utcNow;
    }

    public override DateTimeOffset GetUtcNow()
    {
        return _utcNow;
    }

    public static TimeProviderFijo CrearUtc(int year, int month, int day, int hour = 0, int minute = 0, int second = 0)
    {
        return new TimeProviderFijo(new DateTimeOffset(year, month, day, hour, minute, second, TimeSpan.Zero));
    }
}
