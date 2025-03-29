using DbProvider.Models;

namespace DbProvider.Providers;

public interface IUserProvider
{
    public Task<UserProfile?> getUserProfileAsync(int userId);
    public Task<UserProfile?> updateUserProfileAsync(UserProfile profile);
}