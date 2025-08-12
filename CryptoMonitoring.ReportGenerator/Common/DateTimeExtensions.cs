namespace CryptoMonitoring.ReportGenerator.Common
{
    public static class DateTimeExtensions
    {
        public static DateTime ToUtc(this DateTime dateTime)
        {
            var utc = dateTime.Kind switch
            {
                DateTimeKind.Utc => dateTime,
                DateTimeKind.Local => dateTime.ToUniversalTime(),
                DateTimeKind.Unspecified => DateTime.SpecifyKind(dateTime, DateTimeKind.Utc),
                _ => throw new ArgumentOutOfRangeException(nameof(dateTime.Kind), dateTime.Kind, null)
            };

            return new DateTime(utc.Year, utc.Month, utc.Day, 0, 0, 0, DateTimeKind.Utc);
        }

    }
}
