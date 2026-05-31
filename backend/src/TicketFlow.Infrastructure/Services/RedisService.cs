using System.Text.Json;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using TicketFlow.Application.Common.Interfaces;

namespace TicketFlow.Infrastructure.Services;

public class RedisService : IRedisService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RedisService> _logger;
    private readonly IDatabase _db;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public RedisService(IConnectionMultiplexer redis, ILogger<RedisService> logger)
    {
        _redis = redis;
        _logger = logger;
        _db = redis.GetDatabase();
    }

    // -------------------------------------------------------------------------
    // Cache-aside
    // -------------------------------------------------------------------------

    public async Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
    {
        try
        {
            var value = await _db.StringGetAsync(key);
            if (!value.HasValue) return default;

            var str = (string?)value;
            if (str is null) return default;
            return JsonSerializer.Deserialize<T>(str, JsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Redis GET failed for key {Key} — cache miss", key);
            return default;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken ct = default)
    {
        try
        {
            var json = JsonSerializer.Serialize(value, JsonOptions);
            if (expiry.HasValue)
                await _db.StringSetAsync(key, json, expiry.Value);
            else
                await _db.StringSetAsync(key, json);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Redis SET failed for key {Key} — continuing without cache", key);
        }
    }

    public async Task DeleteAsync(string key, CancellationToken ct = default)
    {
        try
        {
            await _db.KeyDeleteAsync(key);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Redis DELETE failed for key {Key}", key);
        }
    }

    public async Task DeleteByPatternAsync(string pattern, CancellationToken ct = default)
    {
        try
        {
            // SCAN is non-blocking unlike KEYS — safe for production
            var server = _redis.GetServer(_redis.GetEndPoints().First());
            var keys = server.Keys(pattern: pattern).ToArray();
            if (keys.Length > 0)
                await _db.KeyDeleteAsync(keys);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Redis DELETE by pattern failed for pattern {Pattern}", pattern);
        }
    }

    // -------------------------------------------------------------------------
    // Distributed locking
    // -------------------------------------------------------------------------

    public async Task<bool> AcquireLockAsync(string lockKey, TimeSpan expiry, CancellationToken ct = default)
    {
        try
        {
            // SetIfNotExists (NX) + expiry (EX) — atomic in Redis
            return await _db.StringSetAsync(lockKey, "1", (TimeSpan?)expiry, When.NotExists);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Redis lock acquisition failed for key {LockKey}", lockKey);
            return false;
        }
    }

    public async Task ReleaseLockAsync(string lockKey, CancellationToken ct = default)
    {
        try
        {
            await _db.KeyDeleteAsync(lockKey);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Redis lock release failed for key {LockKey}", lockKey);
        }
    }
}
