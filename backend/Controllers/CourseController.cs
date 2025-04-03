using backend.Models;
using DbProvider.Models;
using DbProvider.Providers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

/// <summary>
/// Handles all course-related operations, including course management and student enrollment.
/// </summary>
[ApiController]
[Authorize]
[Route("/api/course")]
public class CourseController : Controller
{
    private readonly ICourseProvider _courseProvider;
    private readonly IUserProvider _userProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="CourseController"/> class.
    /// </summary>
    /// <param name="courseProvider">The course provider service.</param>
    /// <param name="userProvider">The user provider service.</param>
    public CourseController(ICourseProvider courseProvider, IUserProvider userProvider)
    {
        _courseProvider = courseProvider;
        _userProvider = userProvider;
    }

    /// <summary>
    /// Retrieves all courses for the authenticated user.
    /// </summary>
    /// <returns>A list of courses accessible to the user.</returns>
    [HttpGet("get-courses")]
    public async Task<IActionResult> GetCourses()
    {
        var user = await GetCurrentUserAsync();
        if (user == null) return Unauthorized("Invalid user token.");

        var courses = await _courseProvider.GetCourses(user);
        return Ok(courses);
    }

    /// <summary>
    /// Adds a new course for the authenticated teacher.
    /// </summary>
    /// <param name="course">The course to be added.</param>
    /// <returns>The added course.</returns>
    [HttpPost("add-course")]
    public async Task<IActionResult> AddCourse([FromBody] Course course)
    {
        var userId = GetUserId();
        if (userId == null || !IsTeacher()) return Unauthorized("You are not authorized to add a course.");

        course.TeacherId = userId.Value;
        var addedCourse = await _courseProvider.AddCourse(course);
        return Ok(addedCourse);
    }

    /// <summary>
    /// Edits an existing course if owned by the authenticated teacher.
    /// </summary>
    /// <param name="course">The updated course details.</param>
    /// <returns>An Ok result if successful; otherwise, BadRequest.</returns>
    [HttpPut("edit-course")]
    public async Task<IActionResult> EditCourse([FromBody] Course course)
    {
        var userId = GetUserId();
        if (userId == null || !IsTeacher()) return Unauthorized("You are not authorized to edit a course.");

        course.TeacherId = userId.Value;
        var response = await _courseProvider.EditCourse(course);
        return response.IsSuccess ? Ok() : BadRequest("Failed to edit course.");
    }

    /// <summary>
    /// Deletes a course owned by the authenticated teacher.
    /// </summary>
    /// <param name="courseId">The ID of the course to delete.</param>
    /// <returns>An Ok result if successful; otherwise, BadRequest.</returns>
    [HttpDelete("delete-course/{courseId}")]
    public async Task<IActionResult> DeleteCourse(int courseId)
    {
        var userId = GetUserId();
        if (userId == null || !IsTeacher()) return Unauthorized("You are not authorized to delete a course.");

        var response = await _courseProvider.DeleteCourse(courseId);
        return response.IsSuccess ? Ok() : BadRequest("Failed to delete course.");
    }

    /// <summary>
    /// Adds students to a course owned by the authenticated teacher.
    /// </summary>
    /// <param name="request">The request containing course and student IDs.</param>
    /// <returns>An Ok result if successful.</returns>
    [HttpPost("add-student-to-course")]
    public async Task<IActionResult> AddStudentToCourse([FromBody] AddStudentToCourseRequest request)
    {
        var userId = GetUserId();
        if (userId == null || !IsTeacher()) return Unauthorized("You are not authorized.");

        var teacherId = await _courseProvider.GetTeacherId(request.CourseId);
        if (teacherId != userId) return Unauthorized("Not authorized to manage this course.");

        foreach (int studentId in request.StudentIds)
        {
            await _courseProvider.AddStudentToCourse(request.CourseId, studentId);
        }

        return Ok();
    }

    /// <summary>
    /// Removes students from a course owned by the authenticated teacher.
    /// </summary>
    /// <param name="request">The request containing course and student IDs.</param>
    /// <returns>An Ok result if successful; otherwise, BadRequest.</returns>
    [HttpPost("remove-student-from-course")]
    public async Task<IActionResult> RemoveStudentFromCourse([FromBody] RemoveStudentFromCourseRequest request)
    {
        var userId = GetUserId();
        if (userId == null || !IsTeacher()) return Unauthorized("You are not authorized.");

        var teacherId = await _courseProvider.GetTeacherId(request.CourseId);
        if (teacherId != userId) return Unauthorized("Not authorized to manage this course.");

        foreach (int studentId in request.StudentIds)
        {
            var response = await _courseProvider.RemoveStudentFromCourse(request.CourseId, studentId);
            if (!response.IsSuccess)
                return BadRequest("Failed to remove student from course.");
        }

        return Ok();
    }

    /// <summary>
    /// Retrieves students enrolled in a specific course.
    /// </summary>
    /// <param name="courseId">The ID of the course.</param>
    /// <returns>A list of enrolled students.</returns>
    [HttpGet("get-students-in-course/{courseId}")]
    public async Task<IActionResult> GetStudentsInCourse(int courseId)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized("Invalid user token.");

        var students = await _userProvider.GetStudentsInCourse(courseId);
        return Ok(students);
    }

    /// <summary>
    /// Retrieves a course by its ID.
    /// </summary>
    /// <param name="courseId">The ID of the course.</param>
    /// <returns>The course if found; otherwise, NotFound.</returns>
    [HttpGet("get-course-by-id/{courseId}")]
    public async Task<IActionResult> GetCourseById(int courseId)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized("Invalid user token.");

        var course = await _courseProvider.GetCourseById(courseId);
        return course is null ? NotFound("Course not found.") : Ok(course);
    }

    /// <summary>
    /// Extracts the user ID from the current token.
    /// </summary>
    private int? GetUserId()
    {
        var userIdClaim = User.FindFirst("UserId");
        return int.TryParse(userIdClaim?.Value, out var id) ? id : null;
    }

    /// <summary>
    /// Determines if the current user has a "Teacher" role.
    /// </summary>
    private bool IsTeacher() => User.FindFirst("Role")?.Value == "Teacher";

    /// <summary>
    /// Retrieves the full user object for the current user.
    /// </summary>
    private async Task<User?> GetCurrentUserAsync()
    {
        var userId = GetUserId();
        return userId.HasValue ? await _userProvider.getUserByIdAsync(userId.Value) : null;
    }
}
