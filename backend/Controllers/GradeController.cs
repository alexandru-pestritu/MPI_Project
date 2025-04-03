using DbProvider.Models;
using DbProvider.Providers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;



[ApiController]
[Authorize]
[Route("/api/grade")]
public class GradeController : Controller
{
    private readonly IGradeProvider _gradeProvider;
    
    public GradeController(IGradeProvider gradeProvider)
    {
        _gradeProvider = gradeProvider;
    }
    
    [HttpGet]
    [Route("get-grades/{courseId}")]
    public async Task<IActionResult> GetGrades(int courseId)
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
        
        if(User.FindFirst("Role")?.Value != "Teacher")
        {
            return Unauthorized("You are not authorized to add a course.");
        }
        
        var grades = await _gradeProvider.GetGrades(courseId);
        return Ok(grades);
    }
    
    [HttpGet]
    [Route("get-grades-by-student")]
    public async Task<IActionResult> GetGradesByStudent()
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
        
        if(User.FindFirst("Role")?.Value != "Student")
        {
            return Unauthorized("You are not authorized to view the grades.");
        }
        
        var grades = await _gradeProvider.GetGradesByStudent(userId);
        return Ok(grades);
    }
    
    [HttpPost]
    [Route("add-grades")]
    public async Task<IActionResult> AddGrades([FromBody] List<Grade> grades)
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
        
        if(User.FindFirst("Role")?.Value != "Teacher")
        {
            return Unauthorized("You are not authorized to add a course.");
        }
        
        var addedGrades = await _gradeProvider.AddGrades(grades);
        return Ok(addedGrades);
    }
    
    [HttpPut]
    [Route("edit-grade")]
    public async Task<IActionResult> EditGrade([FromBody] Grade grade)
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
        
        if(User.FindFirst("Role")?.Value != "Teacher")
        {
            return Unauthorized("You are not authorized to add a course.");
        }
        
        var result = await _gradeProvider.EditGrade(grade);
        if (!result)
        {
            return BadRequest("Failed to edit grade.");
        }
        return Ok();
    }
    
    [HttpDelete]
    [Route("delete-grade/{gradeId}")]
    public async Task<IActionResult> DeleteGrade(int gradeId)
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
        
        if(User.FindFirst("Role")?.Value != "Teacher")
        {
            return Unauthorized("You are not authorized to add a course.");
        }
        
        var result = await _gradeProvider.DeleteGrade(gradeId);
        if (!result)
        {
            return BadRequest("Failed to delete grade.");
        }
        return Ok();
    }
    
}