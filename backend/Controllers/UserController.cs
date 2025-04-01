using DbProvider.Models;
using DbProvider.Providers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("/api/user")]
[Authorize]
public class UserController : Controller
{
    private readonly IUserProvider _userProvider;
    
    public UserController(IUserProvider userProvider)
    {
        _userProvider = userProvider;
    }
    
    [HttpGet("profile")]
    public async Task<IActionResult> GetUserProfileAsync()
    {
        var userIdClaim = User.FindFirst("UserId");
        if (userIdClaim == null)
        {
            return Unauthorized("No user ID claim found in the token.");
        }
        
        if (!int.TryParse(userIdClaim.Value, out var userId))
        {
            return BadRequest("Invalid user ID claim in the token.");
        }

        var profile = await _userProvider.getUserProfileAsync(userId);
        if (profile == null)
        {
            return NotFound("User profile not found.");
        }

        return Ok(profile);
    }
    
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateUserProfileAsync([FromBody] UserProfile profile)
    {
        var userIdClaim = User.FindFirst("UserId");
        if (userIdClaim == null)
        {
            return Unauthorized("No user ID claim found in the token.");
        }
        
        if (!int.TryParse(userIdClaim.Value, out var userId))
        {
            return BadRequest("Invalid user ID claim in the token.");
        }

        if (profile.UserId != userId)
        {
            return BadRequest("User ID in the token does not match the user ID in the request body.");
        }

        var updatedProfile = await _userProvider.updateUserProfileAsync(profile);
        if (updatedProfile == null)
        {
            return NotFound("User profile not found.");
        }

        return Ok(updatedProfile);
    }
    
    [HttpGet]
    [Route("get-all-students")]
    public async Task<IActionResult> GetAllStudents()
    {
        var students = await _userProvider.GetAllStudents();
        return Ok(students);
    }
}