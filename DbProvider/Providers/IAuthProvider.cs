using DbProvider.Models;
using DbProvider.Models.ProviderOutputs;

namespace DbProvider.Providers;

/// <summary>
/// Defines methods for user authentication and account management.
/// </summary>
public interface IAuthProvider
{
    /// <summary>
    /// Authenticates a user with the given email and password.
    /// </summary>
    /// <param name="email">The email of the user.</param>
    /// <param name="password">The password of the user.</param>
    /// <returns>A task that represents the asynchronous operation. 
    /// The task result contains an <see cref="AuthResponse"/> object.</returns>
    public Task<AuthResponse> AuthenticateAsync(string email, string password);

    /// <summary>
    /// Registers a new user with the specified information.
    /// </summary>
    /// <param name="username">The desired username of the new user.</param>
    /// <param name="email">The email address of the new user.</param>
    /// <param name="password">The desired password.</param>
    /// <param name="confirmPassword">Confirmation of the desired password.</param>
    /// <param name="role">The role of the user represented as a short integer.</param>
    /// <returns>A task that represents the asynchronous operation. 
    /// The task result contains a <see cref="RegisterResponse"/> object.</returns>
    public Task<RegisterResponse> RegisterAsync(string username, string email, string password, string confirmPassword, short role);

    /// <summary>
    /// Verifies a user's account using a verification token.
    /// </summary>
    /// <param name="token">The verification token sent to the user.</param>
    /// <returns>A task that represents the asynchronous operation. 
    /// The task result contains a <see cref="BaseResponse"/> indicating the result.</returns>
    public Task<BaseResponse> VerifyUserAsync(string token);

    /// <summary>
    /// Initiates a password reset process for the user with the specified email.
    /// </summary>
    /// <param name="email">The email of the user who forgot their password.</param>
    /// <returns>A task that represents the asynchronous operation. 
    /// The task result contains a <see cref="ForgotPasswordResponse"/> object.</returns>
    public Task<ForgotPasswordResponse> ForgotPasswordAsync(string email);

    /// <summary>
    /// Changes the user's password using a reset token and the new password.
    /// </summary>
    /// <param name="token">The password reset token.</param>
    /// <param name="password">The new password to set.</param>
    /// <param name="confirmPassword">Confirmation of the new password.</param>
    /// <returns>A task that represents the asynchronous operation. 
    /// The task result contains a <see cref="BaseResponse"/> indicating the result.</returns>
    public Task<BaseResponse> ChangePasswordAsync(string token, string password, string confirmPassword);
}
