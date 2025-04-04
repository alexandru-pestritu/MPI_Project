using DbProvider.Models;
using DbProvider.Providers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

/// <summary>
/// Controller for managing grade-related operations such as viewing, adding, editing, and deleting grades.
/// </summary>
[ApiController]
[Authorize]
[Route("/api/grade")]
public class GradeController : Controller
{
    private readonly IGradeProvider _gradeProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="GradeController"/> class.
    /// </summary>
    /// <param name="gradeProvider">The grade provider service.</param>
    public GradeController(IGradeProvider gradeProvider)
    {
        _gradeProvider = gradeProvider;
    }

    /// <summary>
    /// Retrieves all grades for a given course. Only accessible by teachers.
    /// </summary>
    /// <param name="courseId">The ID of the course.</param>
    /// <returns>A list of grades if authorized; otherwise, an error response.</returns>
    [HttpGet("get-grades/{courseId}")]
    public async Task<IActionResult> GetGrades(int courseId)
    {
        if (!TryValidateTeacher(out var errorResult)) return errorResult;

        var grades = await _gradeProvider.GetGrades(courseId);
        return Ok(grades);
    }

    /// <summary>
    /// Retrieves all grades for the authenticated student.
    /// </summary>
    /// <returns>A list of grades belonging to the student.</returns>
    [HttpGet("get-grades-by-student")]
    public async Task<IActionResult> GetGradesByStudent()
    {
        if (!TryValidateStudent(out var userId, out var errorResult)) return errorResult;

        var grades = await _gradeProvider.GetGradesByStudent(userId);
        return Ok(grades);
    }

    /// <summary>
    /// Adds a list of grades to a course. Only accessible by teachers.
    /// </summary>
    /// <param name="grades">The list of grades to add.</param>
    /// <returns>The added grades or an error response.</returns>
    [HttpPost("add-grades")]
    public async Task<IActionResult> AddGrades([FromBody] List<Grade> grades)
    {
        if (!TryValidateTeacher(out var errorResult)) return errorResult;

        var addedGrades = await _gradeProvider.AddGrades(grades);
        return Ok(addedGrades);
    }

    /// <summary>
    /// Edits a specific grade. Only accessible by teachers.
    /// </summary>
    /// <param name="grade">The grade object containing updated information.</param>
    /// <returns>An Ok result if successful; otherwise, BadRequest.</returns>
    [HttpPut("edit-grade")]
    public async Task<IActionResult> EditGrade([FromBody] Grade grade)
    {
        if (!TryValidateTeacher(out var errorResult)) return errorResult;

        var result = await _gradeProvider.EditGrade(grade);
        return result ? Ok() : BadRequest("Failed to edit grade.");
    }

    /// <summary>
    /// Deletes a specific grade by ID. Only accessible by teachers.
    /// </summary>
    /// <param name="gradeId">The ID of the grade to delete.</param>
    /// <returns>An Ok result if successful; otherwise, BadRequest.</returns>
    [HttpDelete("delete-grade/{gradeId}")]
    public async Task<IActionResult> DeleteGrade(int gradeId)
    {
        if (!TryValidateTeacher(out var errorResult)) return errorResult;

        var result = await _gradeProvider.DeleteGrade(gradeId);
        return result ? Ok() : BadRequest("Failed to delete grade.");
    }

    /// <summary>
    /// Validates that the current user has a "Teacher" role.
    /// </summary>
    private bool TryValidateTeacher(out IActionResult errorResult)
    {
        var userIdClaim = User.FindFirst("UserId");
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out _))
        {
            errorResult = BadRequest("Invalid or missing user ID claim.");
            return false;
        }

        if (User.FindFirst("Role")?.Value != "Teacher")
        {
            errorResult = Unauthorized("You are not authorized to perform this action.");
            return false;
        }

        errorResult = null!;
        return true;
    }

    /// <summary>
    /// Validates that the current user is a "Student" and extracts their user ID.
    /// </summary>
    private bool TryValidateStudent(out int userId, out IActionResult errorResult)
    {
        userId = 0;

        var userIdClaim = User.FindFirst("UserId");
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out userId))
        {
            errorResult = BadRequest("Invalid or missing user ID claim.");
            return false;
        }

        if (User.FindFirst("Role")?.Value != "Student")
        {
            errorResult = Unauthorized("You are not authorized to view grades.");
            return false;
        }

        errorResult = null!;
        return true;
    }
    
    /// <summary>
    /// Upload a CSV file and bulk-insert grades. Only accessible by teachers.
    /// </summary>
    /// <param name="file">The CSV file containing grades to upload.</param>
    /// <returns>The list of inserted <see cref="Grade"/> objects.</returns>
    [HttpPost("bulk-upload")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> BulkUpload([FromForm] IFormFile file)
    {
        if (!TryValidateTeacher(out var errorResult))
        {
            return errorResult;
        }
    
        try
        {
            var result = await _gradeProvider.BulkUploadFromCsvAsync(file);
            return Ok(result);
        }
        catch (ArgumentException argEx)
        {
           
            return BadRequest(argEx.Message);
        }
        catch (IOException ioEx)
        {
           
            return BadRequest(ioEx.Message);
        }
        catch
        {
            return StatusCode(500, "An unexpected error occurred while processing the file.");
        }
    }
    
    /// <summary>
    /// Retrieves all grades for a specific student in a specific course.
    /// </summary>
    /// <param name="courseId">The ID of the course.</param>
    /// <returns>A list of grades for the student in the specified course.</returns>
    [HttpGet("get-student-grades-at-course/{courseId}")]
    public async Task<IActionResult> GetStudentGradesAtCourse(int courseId)
    {
        if (!TryValidateStudent(out var userId, out var errorResult)) return errorResult;

        var grades = await _gradeProvider.GetStudentGradesAtCourse(userId, courseId);
        return Ok(grades);
    }
    
    /// <summary>
    ///  Retrieves the average grade for the authenticated student.
    /// </summary>
    /// <returns>The average grade for the student.</returns>
    [HttpGet("get-average-grade")]
    public async Task<IActionResult> GetAverageGrade()
    {
        if (!TryValidateStudent(out var userId, out var errorResult)) return errorResult;

        var averageGrade = await _gradeProvider.GetAverageGrade(userId);
        return Ok(new { AverageGrade = averageGrade });
    }
}
