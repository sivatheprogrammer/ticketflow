namespace TicketFlow.Application.Common.Interfaces;

public interface IRedisService
{
    // Cache-aside
    Task<T?> GetAsync<T>(string key, CancellationToken ct = default);
    Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken ct = default);
    Task DeleteAsync(string key, CancellationToken ct = default);
    Task DeleteByPatternAsync(string pattern, CancellationToken ct = default);

    // Distributed locking
    Task<bool> AcquireLockAsync(string lockKey, TimeSpan expiry, CancellationToken ct = default);
    Task ReleaseLockAsync(string lockKey, CancellationToken ct = default);
}
