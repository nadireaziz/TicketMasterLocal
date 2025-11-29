using StackExchange.Redis;
using TicketMaster.Domain.Interfaces;

namespace TicketMaster.Infrastructure.Services;

/// <summary>
/// Redis implementation of distributed locking
/// Uses SET NX EX command to prevent race conditions
/// Critical for preventing double-booking in distributed systems
/// </summary>
public class RedisDistributedLockService : IDistributedLockService
{
    private readonly IConnectionMultiplexer _redis;

    public RedisDistributedLockService(IConnectionMultiplexer redis)
    {
        _redis = redis;
    }

    public async Task<bool> AcquireLockAsync(string key, string value, TimeSpan expiration)
    {
        var db = _redis.GetDatabase();

        // SET NX EX - Only set if Not Exists, with Expiration
        // This is atomic and prevents race conditions
        return await db.StringSetAsync(key, value, expiration, When.NotExists);
    }

    public async Task ReleaseLockAsync(string key)
    {
        var db = _redis.GetDatabase();
        await db.KeyDeleteAsync(key);
    }
}
