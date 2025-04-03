using DbProvider.Models;
using DbProvider.Providers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

/// <summary>
/// Controller for handling user profile-related operations.
/// </summary>
[ApiController]
[Route("/api/user")]
[Authorize]
public class UserController : Controller
{
    private readonly IUserProvider _userProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserController"/> class.
    /// </summary>
    /// <param name="userProvider">The user provider service.</param>
    public UserController(IUserProvider userProvider)
    {
        _userProvider = userProvider;
    }

    /// <summary>
    /// Retrieves the currently authenticated user's profile.
    /// </summary>
    /// <returns>The user's profile if found; otherwise, appropriate error response.</returns>
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

    /// <summary>
    /// Updates the currently authenticated user's profile.
    /// </summary>
    /// <param name="profile">The updated user profile information.</param>
    /// <returns>The updated profile if successful; otherwise, an appropriate error response.</returns>
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

    /// <summary>
    /// Retrieves all student user profiles.
    /// </summary>
    /// <returns>A list of all students.</returns>
    [HttpGet]
    [Route("get-all-students")]
    public async Task<IActionResult> GetAllStudents()
    {
        var students = await _userProvider.GetAllStudents();
        return Ok(students);
    }

    /// <summary>
    /// Retrieves a user's profile by their ID.
    /// </summary>
    /// <param name="userId">The ID of the user to retrieve.</param>
    /// <returns>The user's profile if found; otherwise, a NotFound result.</returns>
    [HttpGet]
    [Route("get-user-profile/{userId}")]
    public async Task<IActionResult> GetUserProfile(int userId)
    {
        var profile = await _userProvider.getUserProfileAsync(userId);
        if (profile == null)
        {
            return NotFound("User profile not found.");
        }

        return Ok(profile);
    }
}
