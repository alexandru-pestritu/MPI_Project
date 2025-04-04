using DbProvider.Models;
using Microsoft.AspNetCore.Http;

namespace DbProvider.Providers;

/// <summary>
/// Defines methods for managing grades, including retrieval, modification, addition, and deletion.
/// </summary>
public interface IGradeProvider
{
    /// <summary>
    /// Retrieves all grades for a specific course.
    /// </summary>
    /// <param name="courseId">The ID of the course.</param>
    /// <returns>A task that represents the asynchronous operation. 
    /// The task result contains a list of <see cref="Grade"/> objects for the course.</returns>
    public Task<List<Grade>> GetGrades(int courseId);

    /// <summary>
    /// Edits an existing grade.
    /// </summary>
    /// <param name="grade">The grade to be updated.</param>
    /// <returns>A task that represents the asynchronous operation. 
    /// The task result contains <c>true</c> if the update was successful; otherwise, <c>false</c>.</returns>
    public Task<bool> EditGrade(Grade grade);

    /// <summary>
    /// Adds a list of grades.
    /// </summary>
    /// <param name="grades">The grades to add.</param>
    /// <returns>A task that represents the asynchronous operation. 
    /// The task result contains a list of added <see cref="Grade"/> objects, 
    /// or <c>null</c> entries if any failed.</returns>
    public Task<List<Grade?>> AddGrades(List<Grade> grades);

    /// <summary>
    /// Deletes a grade by its ID.
    /// </summary>
    /// <param name="gradeId">The ID of the grade to delete.</param>
    /// <returns>A task that represents the asynchronous operation. 
    /// The task result contains <c>true</c> if the deletion was successful; otherwise, <c>false</c>.</returns>
    public Task<bool> DeleteGrade(int gradeId);

    /// <summary>
    /// Retrieves all grades for a specific student.
    /// </summary>
    /// <param name="studentId">The ID of the student.</param>
    /// <returns>A task that represents the asynchronous operation. 
    /// The task result contains a list of <see cref="Grade"/> objects for the student, or <c>null</c> if none found.</returns>
    public Task<List<Grade>?> GetGradesByStudent(int studentId);
    
    /// <summary>
    /// Reads grades from a CSV file, validates them, and bulk-inserts valid entries.
    /// </summary>
    /// <param name="file">The uploaded CSV file (e.g. from an Angular front-end).</param>
    /// <returns>A list of created <see cref="Grade"/> objects (with IDs), or <c>null</c> for rows that failed validation.</returns>
    Task<List<Grade?>> BulkUploadFromCsvAsync(IFormFile file);
    
    
    /// <summary>
    /// Retrieves all grades for a specific student in a specific course.
    /// </summary>
    /// <param name="studentId">The ID of the student.</param>
    /// <param name="courseId">The ID of the course.</param>
    /// <returns>A task that represents the asynchronous operation.
    /// The task result contains a list of <see cref="Grade"/> objects for the student in the course.</returns>
    Task<List<Grade>> GetStudentGradesAtCourse(int studentId, int courseId);
    
    /// <summary>
    /// Retrieves the average grade for a specific student.
    /// </summary>
    /// <param name="studentId">The ID of the student.</param>
    /// <returns>A task that represents the asynchronous operation.
    /// The task result contains the average grade as a <see cref="float"/>.</returns>
    Task<float> GetAverageGrade(int studentId);
    
    /// <summary>
    /// Retrieves the history of a specific grade.
    /// </summary>
    /// <param name="gradeId">The ID of the grade.</param>
    /// <returns>A task that represents the asynchronous operation.
    /// The task result contains a list of <see cref="GradeHistory"/> objects representing the history of the grade.</returns>
    Task<List<GradeHistory>> GetGradeHistory(int gradeId);
}
