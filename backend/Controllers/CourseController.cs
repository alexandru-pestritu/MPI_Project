using backend.Models;
using DbProvider.Models;
using DbProvider.Providers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Authorize]
[Route("/api/course")]

public class CourseController : Controller
{
    private readonly ICourseProvider _courseProvider;
    private readonly IUserProvider _userProvider;
    
    public CourseController(ICourseProvider courseProvider, IUserProvider userProvider)
    {
        _courseProvider = courseProvider;
        _userProvider = userProvider;
    }
    
    
    [HttpGet]
    [Route("get-courses")]
    public async Task<IActionResult> GetCourses()
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
        User? user = await _userProvider.getUserByIdAsync(userId);
        if (user is null)
        {
            return BadRequest("User not found.");
        }
        
        var courses = await _courseProvider.GetCourses(user);
        return Ok(courses);
    }
    
    
    [HttpPost]
    [Route("add-course")]
    public async Task<IActionResult> AddCourse([FromBody] Course course)
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
        
        course.TeacherId = userId;
        
        var addedCourse = await _courseProvider.AddCourse(course);
        return Ok(addedCourse);
    }
    
    [HttpPut]
    [Route("edit-course")]
    public async Task<IActionResult> EditCourse([FromBody] Course course)
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
            return Unauthorized("You are not authorized to edit a course.");
        }
        
        course.TeacherId = userId;
        
        var response = await _courseProvider.EditCourse(course);
        
        if (!response.IsSuccess)
        {
            return BadRequest("Failed to edit course.");
        }
        return Ok();
    }
    
    [HttpDelete]
    [Route("delete-course/{courseId}")]
    public async Task<IActionResult> DeleteCourse(int courseId)
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
            return Unauthorized("You are not authorized to delete a course.");
        }
        
        var response = await _courseProvider.DeleteCourse(courseId);
        if (!response.IsSuccess)
        {
            return BadRequest("Failed to delete course.");
        }
        return Ok();
    }
    
    [HttpPost]
    [Route("add-student-to-course")]
    public async Task<IActionResult> AddStudentToCourse([FromBody] AddStudentToCourseRequest request)
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
            return Unauthorized("You are not authorized to add a student to a course.");
        }
        
        int teacherId = await _courseProvider.GetTeacherId(request.CourseId);

        if (teacherId != userId)
        {
            return Unauthorized("Not authorized to add a student to this course.");
        }
        foreach(int studentId in request.StudentIds)
        {
            var response = await _courseProvider.AddStudentToCourse(request.CourseId, studentId);
        }
        
        
        return Ok();
    }
    
    [HttpPost]
    [Route("remove-student-from-course")]
    public async Task<IActionResult> RemoveStudentFromCourse([FromBody] RemoveStudentFromCourseRequest request)
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
            return Unauthorized("You are not authorized to remove a student from a course.");
        }
        
        int teacherId = await _courseProvider.GetTeacherId(request.CourseId);

        if (teacherId != userId)
        {
            return Unauthorized("Not authorized to remove a student from this course.");
        }
        
        foreach(int studentId in request.StudentIds)
        {
            var response = await _courseProvider.RemoveStudentFromCourse(request.CourseId, studentId);
            if (!response.IsSuccess)
            {
                return BadRequest("Failed to remove student from course.");
            }
        }
        
        
        return Ok();
    }
    
    [HttpGet]
    [Route("get-students-in-course/{courseId}")]
    public async Task<IActionResult> GetStudentsInCourse(int courseId)
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
        
        
        var students = await _userProvider.GetStudentsInCourse(courseId);
        
        return Ok(students);
    }
    
    [HttpGet]
    [Route("get-course-by-id/{courseId}")]
    public async Task<IActionResult> GetCourseById(int courseId)
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
        
        
        var course = await _courseProvider.GetCourseById(courseId);

        if(course is null)
        {
            return NotFound("Course not found.");
        }
       
        return Ok(course);
    }
    
    
}