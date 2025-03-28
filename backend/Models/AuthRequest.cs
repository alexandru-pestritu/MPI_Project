namespace backend.Models;

public class AuthRequest
{
    public string Email { get; set; }
    public string Password { get; set; }
    
    public AuthRequest(string email, string password)
    {
        Email = email;
        Password = password;
    }
}