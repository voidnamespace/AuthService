namespace AuthService.Domain.ValueObjects;
using BCrypt.Net;

public class PasswordVO
{
    private string _hash = string.Empty;
    public string Hash => _hash;

    private PasswordVO() { } // clear private contructor for entity to let  EF core work  and aswell for VO objects

    public static PasswordVO FromHash(string hash)
    {
        return new PasswordVO { _hash = hash };
    }

    public PasswordVO(string plainPassword)
    {
        if (string.IsNullOrEmpty(plainPassword))
            throw new ArgumentNullException(nameof(plainPassword));
        if (plainPassword.Length < 6)
            throw new ArgumentException("Password must be at least 6 characters");
        _hash = BCrypt.HashPassword(plainPassword);
    }
}
