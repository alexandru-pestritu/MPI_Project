using DbProvider.Models;
using DbProvider.Models.ProviderOutputs;

namespace DbProvider.Providers;

public interface IAuthProvider
{
    public Task<AuthResponse> AuthenticateAsync(string email, string password);
    public Task<RegisterResponse> RegisterAsync(string username,string email, string password,string confirmPassword, short role);
    public Task<BaseResponse> VerifyUserAsync(string token);
    public Task<ForgotPasswordResponse> ForgotPasswordAsync(string email);
    public Task<BaseResponse> ChangePasswordAsync(string token, string password, string confirmPassword);
}