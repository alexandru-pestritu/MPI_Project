namespace DbProvider.Models;

public class UserProfile
{
    public int Id { get; set; }
    
    public int UserId { get; set; }
    
    public string FirstName { get; set; }
    
    public string LastName { get; set; }
    
    public string Bio { get; set; }
    
    public string ProfilePicture { get; set; }
    
    public UserProfile(int id, int userId, string firstName, string lastName, string bio, string profilePicture)
    {
        Id = id;
        UserId = userId;
        FirstName = firstName;
        LastName = lastName;
        Bio = bio;
        ProfilePicture = profilePicture;
    }
}