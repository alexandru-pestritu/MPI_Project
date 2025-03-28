namespace DbProvider.Models;

public class User
{
    public int Id { get; set; }
    
    public string Username { get; set; }
    
    public string Password { get; set; }
    
    public string Email { get; set; }
    
    public short Role { get; set; }
    
    
    public User(int id, string username, string password, string email, short role)
    {
        Id = id;
        Username = username;
        Password = password;
        Email = email;
        Role = role;
    }
}