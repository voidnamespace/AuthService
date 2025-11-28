namespace AuthService.Domain.ValueObjects;
using BCrypt.Net;

public class PasswordVO
{
    private string _hash = string.Empty;
    public string Hash => _hash;

    private PasswordVO() { } 

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
