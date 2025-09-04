using Microsoft.AspNetCore.SignalR;


namespace CryptoMonitoring.NotificationService
{
    public class QueryStringUserIdProvider : IUserIdProvider
    {
        public string? GetUserId(HubConnectionContext connection)
             => connection.GetHttpContext()?.Request.Query["userId"];
    }
}
