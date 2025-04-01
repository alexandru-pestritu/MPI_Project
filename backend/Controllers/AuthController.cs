using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using backend.Models;
using backend.Services;
using DbProvider.Models;
using DbProvider.Providers;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace backend.Controllers;

[ApiController]
[Route("/api/auth")]
public class AuthController : Controller
{
    private readonly IAuthProvider _authProvider;
    private readonly IConfiguration _config;
    private readonly IAppEmailService _emailService;

    public AuthController(IAuthProvider authProvider, IConfiguration config, IAppEmailService emailService)
    {
        _authProvider = authProvider;
        _config = config;
        _emailService = emailService;
    }

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
        
        if(roleString is null)
        {
            return Unauthorized("Invalid role");
        }
        var token = GenerateJwtToken(response.User, roleString);
        
        return Ok(new { token });
    }
    
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

    [HttpPost]
    [Route("register")]
    public async Task<IActionResult> RegisterAsync([FromBody] RegisterRequest request)
    {
        var result = await _authProvider.RegisterAsync(request.Username, request.Email, request.Password,request.ConfirmPassword, request.Role );

        if (!result.IsSuccess)
        {
            return BadRequest(result.Message);
        }
        
        string verifEmail =  $"{Request.Scheme}://{Request.Host}/api/auth/verify-email/{result.Token}";
        
        await  _emailService.SendVerificationEmailAsync(request.Email, verifEmail);

        return Ok();
    }

    [HttpGet]
    [Route("verify-email/{token}")]
    public async Task<IActionResult> VerifyEmailAsync(string token)
    {
        var result = await _authProvider.VerifyUserAsync(token);
        
        if(!result.IsSuccess)
        {
            return BadRequest(result.Message);
        }

        return Redirect($"{_config["Frontend:Url"]}/login");
    }
    
    private string GenerateJwtToken(User user, string role)
    {
        var secretKey = _config["Jwt:Key"];
        var issuer = _config["Jwt:Issuer"];

        var keyBytes = Encoding.UTF8.GetBytes(secretKey);
        var securityKey = new SymmetricSecurityKey(keyBytes);

        var credentials = new SigningCredentials(
            securityKey,
            SecurityAlgorithms.HmacSha256
        );
        
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