namespace AuthService.Domain.ValueObjects;

public class EmailVO
{
    public string _email { get; private set; } = string.Empty; 
    private EmailVO() { } // clear private contructor for entity to let  EF core work and aswell for VO objects

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
