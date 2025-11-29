namespace TicketMaster.Domain.Entities;

public class RefreshToken
{
    public int TokenId { get; set; }
    public int UserId { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; }
    public DateTime? RevokedAt { get; set; }
    public string? DeviceName { get; set; } // "Chrome on Windows", "iPhone", etc.
    public string? IpAddress { get; set; }

    // Navigation property
    public User User { get; set; } = null!;

    // Computed property
    public bool IsActive => RevokedAt == null && ExpiresAt > DateTime.UtcNow;
}
