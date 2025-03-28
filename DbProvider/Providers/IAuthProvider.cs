using DbProvider.Models;
using DbProvider.Models.ProviderOutputs;

namespace DbProvider.Providers;

public interface IAuthProvider
{
    public Task<User?> AuthenticateAsync(string username, string password);
    public Task<RegisterResponse> RegisterAsync(string username,string email, string password,string confirmPassword, short role);
}