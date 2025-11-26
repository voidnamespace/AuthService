namespace AuthService.Domain.Entities;

public class RefreshToken
{

    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string Token { get; set; } = string.Empty;

    public DateTime ExpiryDate { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsRevoked { get; set; } = false;

    public DateTime? RevokedAt { get; set; }

    public virtual User User { get; set; } = null!;

    public bool IsActive()
    {
        return !IsRevoked && ExpiryDate > DateTime.UtcNow;
    }

    public bool IsExpired()
    {
        return DateTime.UtcNow >= ExpiryDate;
    }
}
