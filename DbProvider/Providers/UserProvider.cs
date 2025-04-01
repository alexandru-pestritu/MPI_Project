using DbProvider.Database;
using DbProvider.Models;

namespace DbProvider.Providers;

public class UserProvider : IUserProvider
{
    private readonly IDbManager _manager;
    
    public UserProvider(IDbManager manager)
    {
        _manager = manager;
    }
    
    public async Task<UserProfile?> getUserProfileAsync(int userId)
    {
        string query = "SELECT * FROM UserProfiles WHERE UserId = @UserId";
        return await _manager.ReadObjectOfTypeAsync(query, ConvertUserProfile, new KeyValuePair<string, object>("UserId", userId));
    }

    public async Task<UserProfile?> updateUserProfileAsync(UserProfile profile)
    {
        bool res = await _manager.UpdateAsync("UserProfiles",
            new KeyValuePair<string, object>("UserId", profile.UserId),
            new KeyValuePair<string, object>("FirstName", profile.FirstName),
            new KeyValuePair<string, object>("LastName", profile.LastName),
            new KeyValuePair<string, object>("Bio", profile.Bio));

        if (!res)
            return null;
        
        return await getUserProfileAsync(profile.UserId);
    }

    public async Task<User?> getUserByIdAsync(int userId)
    {
        string query = "SELECT * FROM Users WHERE Id = @Id";

        return await _manager.ReadObjectOfTypeAsync(query, ConvertUser, new KeyValuePair<string, object>("Id", userId));
    }

    public Task<User?> getUserByEmailAsync(string email)
    {
        string query = "SELECT * FROM Users WHERE Email = @Email";

        return _manager.ReadObjectOfTypeAsync(query, ConvertUser, new KeyValuePair<string, object>("Email", email));
    }
    
    public async Task<List<UserProfile>> GetStudentsInCourse(int courseId)
    {
        string query = "SELECT * FROM Users join CourseStudentLink on Users.Id = CourseStudentLink.StudentId WHERE CourseStudentLink.CourseId = @CourseId";
        List<User> users = await _manager.ReadListOfTypeAsync(query, ConvertUser, new KeyValuePair<string, object>("CourseId", courseId));
        
        List<UserProfile> profiles = new List<UserProfile>();
        foreach (var user in users)
        {
            var profile = await getUserProfileAsync(user.Id);
            if (profile != null)
            {
                profiles.Add(profile);
            }
        }

        return profiles;
    }

    public async Task<List<UserProfile>> GetAllStudents()
    {
        string query = "SELECT * FROM Users WHERE Role = 0";
        List<User> users = await _manager.ReadListOfTypeAsync(query, ConvertUser);
        List<UserProfile> profiles = new List<UserProfile>();
        foreach (var user in users)
        {
            var profile = await getUserProfileAsync(user.Id);
            if (profile != null)
            {
                profiles.Add(profile);
            }
        }

        return profiles;
    }

    private UserProfile ConvertUserProfile(object[] values)
    {
        int Id = (int)values[0];
        int UserId = (int)values[1];
        string FirstName = (string)values[2];
        string LastName = (string)values[3];
        string Bio = (string)values[4];
        return new UserProfile(Id, UserId, FirstName, LastName, Bio);
    }
    
    private User ConvertUser(object[] values)
    {
        int id = (int)values[0];
        string username = (string)values[1];
        string password = (string)values[2];
        string email = (string)values[3];
        short role = (short)values[4];
        bool isVerified = (bool)values[5];
        return new User(id, username, password, email, role, isVerified);
    }
}