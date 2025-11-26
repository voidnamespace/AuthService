namespace AuthService.Domain.ValueObjects;

public class EmailVO
{
    public string _email { get; private set; } = string.Empty; 
    private EmailVO() { }

    public EmailVO(string email)
    {
        if (string.IsNullOrEmpty(email))
            throw new ArgumentNullException("email");
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            if (addr.Address != email)
                throw new ArgumentException("Invalid email format");
        }
        catch
        {
            throw new ArgumentException("Invalid email format");
        }
        _email = email;
    }
}
