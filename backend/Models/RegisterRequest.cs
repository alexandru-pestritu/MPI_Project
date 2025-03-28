namespace backend.Models;

public class RegisterRequest
{
    public string Username { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public string ConfirmPassword { get; set; }
    public short Role { get; set; }
    
    public RegisterRequest(string username, string password, string email,string confirmPassword,short role)
    {
        Username = username;
        Email = email;
        Password = password;
        ConfirmPassword = confirmPassword;
        Role = role;
    }
}