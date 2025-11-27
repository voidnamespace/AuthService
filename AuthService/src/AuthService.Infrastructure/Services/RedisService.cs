using StackExchange.Redis;
using System.Text.Json;

namespace AuthService.Infrastructure.Services
{
    public class RedisService
    {
        private readonly IDatabase _db;

        public RedisService(IConnectionMultiplexer redis)
        {
            _db = redis.GetDatabase();
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
        {
            var json = JsonSerializer.Serialize(value);

            if (expiry.HasValue)
            {
                await _db.StringSetAsync(key, json, expiry.Value);
            }
            else
            {
                await _db.StringSetAsync(key, json);
            }
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            var value = await _db.StringGetAsync(key);
            if (!value.HasValue)
                return default;

            return JsonSerializer.Deserialize<T>(value!);
        }

        public async Task RemoveAsync(string key)
        {
            await _db.KeyDeleteAsync(key);
        }
    }
}