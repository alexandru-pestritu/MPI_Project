using DbProvider.Models;

namespace DbProvider.Providers;

public interface IUserProvider
{
    public Task<UserProfile?> getUserProfileAsync(int userId);
    public Task<UserProfile?> updateUserProfileAsync(UserProfile profile);
    
    public Task<User?> getUserByIdAsync(int userId);
    
    public Task<User?> getUserByEmailAsync(string email);
    
    public Task<List<UserProfile>> GetStudentsInCourse(int courseId);
    
    public Task<List<UserProfile>> GetAllStudents();
}