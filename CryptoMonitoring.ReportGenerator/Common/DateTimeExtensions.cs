namespace CryptoMonitoring.ReportGenerator.Common
{
    public static class DateTimeExtensions
    {
        public static DateTime? ToUtc(this DateTime? dt)
        {
            if (dt == null) return null;
            return DateTime.SpecifyKind(dt.Value, DateTimeKind.Utc);
        }

        public static DateTime ToUtc(this DateTime dt)
        {
            return DateTime.SpecifyKind(dt, DateTimeKind.Utc);
        }
    }
}
