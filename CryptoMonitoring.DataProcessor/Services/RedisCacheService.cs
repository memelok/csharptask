using System.Text.Json;
using System;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;

namespace CryptoMonitoring.DataProcessor.Services
{
    public class RedisCacheService
    {
        private readonly IDatabase _db;

        public RedisCacheService(IConfiguration cfg)
        {
            var conn = cfg.GetConnectionString("Redis") ?? "redis:6379";
            _db = ConnectionMultiplexer.Connect(conn).GetDatabase();
        }

        public void Set(string key, object value)
        {
            var json = JsonSerializer.Serialize(value);
            _db.StringSet(key, json, TimeSpan.FromMinutes(10));
        }

        public T? Get<T>(string key)
        {
            var val = _db.StringGet(key);
            return val.HasValue
                ? JsonSerializer.Deserialize<T>(val!)
                : default;
        }
    }
}
