namespace TicketMaster.Domain.Interfaces;

/// <summary>
/// Interface for distributed locking (prevents double-booking)
/// </summary>
public interface IDistributedLockService
{
    Task<bool> AcquireLockAsync(string key, string value, TimeSpan expiration);
    Task ReleaseLockAsync(string key);
}
