using backend.Models;
using DbProvider.Models;
using DbProvider.Providers;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("/api/auth")]
public class AuthController : Controller
{
    private readonly IAuthProvider _authProvider;

    public AuthController(IAuthProvider authProvider)
    {
        _authProvider = authProvider;
    }

    [HttpPost]
    [Route("login")]
    public async Task<IActionResult> AuthenticateAsync([FromBody] AuthRequest request)
    {
        User? response = await _authProvider.AuthenticateAsync(request.Username, request.Password);
        if (response is null)
        {
            return NotFound("User not found");
        }

        return Ok(response);
    }
}