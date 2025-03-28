namespace backend.Models;

public class RegisterRequest
{
    public string Username { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    
    public RegisterRequest(string username, string password, string email)
    {
        Username = username;
        Email = email;
        Password = password;
    }
}