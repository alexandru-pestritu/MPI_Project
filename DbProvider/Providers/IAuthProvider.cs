using DbProvider.Models;

namespace DbProvider.Providers;

public interface IAuthProvider
{
    public Task<User?> AuthenticateAsync(string username, string password);
}