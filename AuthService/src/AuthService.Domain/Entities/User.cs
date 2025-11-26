using AuthService.Domain.ValueObjects;
using AuthService.Domain.Enums;

namespace AuthService.Domain.Entities;

public class User
{

    public Guid Id { get; set; }
    public EmailVO Email { get; private set; } = null!;

    public PasswordVO PasswordHash { get; private set; } = null!;

    public Roles Role { get; private set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public bool IsActive { get; set; } = true;

    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

    private User() { }
    public User(EmailVO email, PasswordVO password, Roles role = Roles.Customer)
    {
        Id = Guid.NewGuid();
        Email = email;
        PasswordHash = password;
        Role = role;
        CreatedAt = DateTime.UtcNow;
    }

    public void ChangeEmail(EmailVO newEmail)
    {
        Email = newEmail;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ChangePassword(PasswordVO newPassword)
    {
        PasswordHash = newPassword;
        UpdatedAt = DateTime.UtcNow;
    }
    public void ChangeRole(Roles newRole)
    {
        Role = newRole;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;


}
