namespace DbProvider.Models;

public class User
{
    public int Id { get; set; }
    
    public string Username { get; set; }
    
    public string Password { get; set; }
    
    public string Email { get; set; }
    
    public short Role { get; set; }
    
    public bool IsVerified { get; set; }
    
    
    public User(int id, string username, string password, string email, short role, bool isVerified)
    {
        Id = id;
        Username = username;
        Password = password;
        Email = email;
        Role = role;
        IsVerified = isVerified;
    }
}