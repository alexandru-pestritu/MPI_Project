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
    
    private UserProfile ConvertUserProfile(object[] values)
    {
        int Id = (int)values[0];
        int UserId = (int)values[1];
        string FirstName = (string)values[2];
        string LastName = (string)values[3];
        string Bio = (string)values[4];
        return new UserProfile(Id, UserId, FirstName, LastName, Bio);
    }
}