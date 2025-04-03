using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using backend.Models;
using backend.Services;
using DbProvider.Models;
using DbProvider.Providers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace backend.Controllers;

/// <summary>
/// Handles authentication and account-related operations such as login, registration,
/// password reset, and email verification.
/// </summary>
[ApiController]
[Route("/api/auth")]
public class AuthController : Controller
{
    private readonly IAuthProvider _authProvider;
    private readonly IConfiguration _config;
    private readonly IAppEmailService _emailService;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthController"/> class.
    /// </summary>
    /// <param name="authProvider">The authentication provider.</param>
    /// <param name="config">The application configuration settings.</param>
    /// <param name="emailService">The email service for sending account-related emails.</param>
    public AuthController(IAuthProvider authProvider, IConfiguration config, IAppEmailService emailService)
    {
        _authProvider = authProvider;
        _config = config;
        _emailService = emailService;
    }

    /// <summary>
    /// Authenticates a user and returns a JWT token if credentials are valid.
    /// </summary>
    /// <param name="request">The authentication request containing email and password.</param>
    /// <returns>A JWT token if authentication is successful; otherwise, an Unauthorized result.</returns>
    [HttpPost]
    [Route("login")]
    public async Task<IActionResult> AuthenticateAsync([FromBody] AuthRequest request)
    {
        var response = await _authProvider.AuthenticateAsync(request.Email, request.Password);
        if (!response.IsSuccess)
        {
            return Unauthorized("Invalid credentials");
        }

        string? roleString = Enum.GetName(typeof(Role), response.User.Role);
        if (roleString is null)
        {
            return Unauthorized("Invalid role");
        }

        var token = GenerateJwtToken(response.User, roleString);
        return Ok(new { token });
    }

    /// <summary>
    /// Initiates a password reset process by sending a reset link to the user's email.
    /// </summary>
    /// <param name="request">The forgot password request containing the user's email.</param>
    /// <returns>An HTTP 200 OK if successful; otherwise, a Bad Request.</returns>
    [HttpPost]
    [Route("forgot-password")]
    public async Task<IActionResult> ForgotPasswordAsync([FromBody] ForgotPasswordRequest request)
    {
        var result = await _authProvider.ForgotPasswordAsync(request.Email);
        if (!result.IsSuccess)
        {
            return BadRequest(result.Message);
        }

        string resetPasswordLink = $"{_config["Frontend:Url"]}/reset-password/{result.Token}";
        await _emailService.SendPasswordResetEmailAsync(request.Email, resetPasswordLink);

        return Ok();
    }

    /// <summary>
    /// Resets a user's password using a provided token.
    /// </summary>
    /// <param name="request">The reset password request containing token and new password details.</param>
    /// <returns>An HTTP 200 OK if successful; otherwise, a Bad Request.</returns>
    [HttpPost]
    [Route("reset-password")]
    public async Task<IActionResult> ResetPasswordAsync([FromBody] ResetPasswordRequest request)
    {
        var result = await _authProvider.ChangePasswordAsync(request.Token, request.Password, request.ConfirmPassword);
        if (!result.IsSuccess)
        {
            return BadRequest(result.Message);
        }

        return Ok();
    }

    /// <summary>
    /// Registers a new user and sends a verification email.
    /// </summary>
    /// <param name="request">The registration request containing user details.</param>
    /// <returns>An HTTP 200 OK if successful; otherwise, a Bad Request.</returns>
    [HttpPost]
    [Route("register")]
    public async Task<IActionResult> RegisterAsync([FromBody] RegisterRequest request)
    {
        var result = await _authProvider.RegisterAsync(request.Username, request.Email, request.Password, request.ConfirmPassword, request.Role);

        if (!result.IsSuccess)
        {
            return BadRequest(result.Message);
        }

        string verifEmail = $"{Request.Scheme}://{Request.Host}/api/auth/verify-email/{result.Token}";
        await _emailService.SendVerificationEmailAsync(request.Email, verifEmail);

        return Ok();
    }

    /// <summary>
    /// Verifies a user's email using the provided token.
    /// </summary>
    /// <param name="token">The email verification token.</param>
    /// <returns>Redirects to the frontend login page on success; otherwise, returns a Bad Request.</returns>
    [HttpGet]
    [Route("verify-email/{token}")]
    public async Task<IActionResult> VerifyEmailAsync(string token)
    {
        var result = await _authProvider.VerifyUserAsync(token);
        if (!result.IsSuccess)
        {
            return BadRequest(result.Message);
        }

        return Redirect($"{_config["Frontend:Url"]}/login");
    }

    /// <summary>
    /// Generates a JWT token for an authenticated user.
    /// </summary>
    /// <param name="user">The authenticated user.</param>
    /// <param name="role">The user's role as a string.</param>
    /// <returns>A signed JWT token.</returns>
    private string GenerateJwtToken(User user, string role)
    {
        var secretKey = _config["Jwt:Key"];
        var issuer = _config["Jwt:Issuer"];

        var keyBytes = Encoding.UTF8.GetBytes(secretKey);
        var securityKey = new SymmetricSecurityKey(keyBytes);

        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim("UserId", user.Id.ToString()),
            new Claim("Role", role),
        };

        var tokenDescriptor = new JwtSecurityToken(
            issuer: issuer,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(24),
            signingCredentials: credentials
        );

        var jwtHandler = new JwtSecurityTokenHandler();
        return jwtHandler.WriteToken(tokenDescriptor);
    }
}
