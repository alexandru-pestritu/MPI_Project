using DbProvider.Models;

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
}
