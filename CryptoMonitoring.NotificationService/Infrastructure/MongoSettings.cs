namespace CryptoMonitoring.NotificationService.Models
{
    public class MongoSettings
    {
        public string DatabaseName { get; set; } = "notifications";
        public string AlertRulesCollectionName { get; set; } = "alert_rules";
    }
}
