using StackExchange.Redis;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using AuthService.Application.Common.Interfaces;

namespace AuthService.Infrastructure.Services;

public class RedisService : IRedisService
{
    private readonly IDatabase _db;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly ILogger<RedisService> _logger;

    private readonly string _prefix = "authservice:";

    public RedisService(IConnectionMultiplexer redis, ILogger<RedisService> logger)
    {
        _db = redis.GetDatabase();
        _logger = logger;

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    }

    private string BuildKey(string key)
        => $"{_prefix}{key}";

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
    {
        try
        {
            if (value == null)
            {
                await _db.KeyDeleteAsync(BuildKey(key));
                return;
            }

            var json = JsonSerializer.Serialize(value, _jsonOptions);

            await _db.StringSetAsync(
                BuildKey(key),
                json,
                expiry: expiry,
                when: When.Always,
                flags: CommandFlags.None
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Redis SetAsync failed. Key={Key}", key);
            throw;
        }
    }


    public async Task<T?> GetAsync<T>(string key)
    {
        try
        {
            var value = await _db.StringGetAsync(BuildKey(key));

            if (!value.HasValue)
                return default;

            return JsonSerializer.Deserialize<T>(value!, _jsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Redis GetAsync failed. Key={Key}", key);
            return default;
        }
    }

    public async Task RemoveAsync(string key)
    {
        try
        {
            await _db.KeyDeleteAsync(BuildKey(key));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Redis RemoveAsync failed. Key={Key}", key);
            throw;
        }
    }

}
