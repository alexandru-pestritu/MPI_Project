using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using backend.Models;
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

    public AuthController(IAuthProvider authProvider, IConfiguration config)
    {
        _authProvider = authProvider;
        _config = config;
    }

    [HttpPost]
    [Route("login")]
    public async Task<IActionResult> AuthenticateAsync([FromBody] AuthRequest request)
    {
        User? user = await _authProvider.AuthenticateAsync(request.Username, request.Password);
        if (user is null)
        {
            return Unauthorized("Invalid credentials");
        }
        
        string? roleString = Enum.GetName(typeof(Role), user.Role);
        
        if(roleString is null)
        {
            return Unauthorized("Invalid role");
        }
        var token = GenerateJwtToken(user, roleString);
        
        return Ok(new { token });
    }

    [HttpPost]
    [Route("register")]
    public async Task<IActionResult> RegisterAsync([FromBody] RegisterRequest request)
    {
        var result = await _authProvider.RegisterAsync(request.Username, request.Email, request.Password, (short)Role.Student );

        if (!result.IsSuccess)
        {
            return BadRequest(result.Message);
        }

        return Ok();
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
            new Claim(ClaimTypes.Role, role),
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