using DbProvider.Models;
using DbProvider.Models.ProviderOutputs;

namespace DbProvider.Providers;

/// <summary>
/// Defines methods for managing course data and student-course associations.
/// </summary>
public interface ICourseProvider
{
    /// <summary>
    /// Retrieves a list of courses available to the specified user.
    /// </summary>
    /// <param name="user">The user for whom to retrieve courses.</param>
    /// <returns>A task that represents the asynchronous operation. 
    /// The task result contains a list of <see cref="Course"/> objects.</returns>
    public Task<List<Course>> GetCourses(User user);

    /// <summary>
    /// Retrieves a course by its unique identifier.
    /// </summary>
    /// <param name="courseId">The ID of the course.</param>
    /// <returns>A task that represents the asynchronous operation. 
    /// The task result contains a <see cref="Course"/> object if found; otherwise, null.</returns>
    public Task<Course?> GetCourseById(int courseId);

    /// <summary>
    /// Adds a new course.
    /// </summary>
    /// <param name="course">The course to add.</param>
    /// <returns>A task that represents the asynchronous operation. 
    /// The task result contains the added <see cref="Course"/> object.</returns>
    public Task<Course> AddCourse(Course course);

    /// <summary>
    /// Edits an existing course.
    /// </summary>
    /// <param name="course">The course with updated information.</param>
    /// <returns>A task that represents the asynchronous operation. 
    /// The task result contains a <see cref="BaseResponse"/> indicating success or failure.</returns>
    public Task<BaseResponse> EditCourse(Course course);

    /// <summary>
    /// Deletes a course by its unique identifier.
    /// </summary>
    /// <param name="courseId">The ID of the course to delete.</param>
    /// <returns>A task that represents the asynchronous operation. 
    /// The task result contains a <see cref="BaseResponse"/> indicating success or failure.</returns>
    public Task<BaseResponse> DeleteCourse(int courseId);

    /// <summary>
    /// Adds a student to a course.
    /// </summary>
    /// <param name="courseId">The ID of the course.</param>
    /// <param name="studentId">The ID of the student to add.</param>
    /// <returns>A task that represents the asynchronous operation. 
    /// The task result contains a <see cref="BaseResponse"/> indicating the result.</returns>
    public Task<BaseResponse> AddStudentToCourse(int courseId, int studentId);

    /// <summary>
    /// Removes a student from a course.
    /// </summary>
    /// <param name="courseId">The ID of the course.</param>
    /// <param name="studentId">The ID of the student to remove.</param>
    /// <returns>A task that represents the asynchronous operation. 
    /// The task result contains a <see cref="BaseResponse"/> indicating the result.</returns>
    public Task<BaseResponse> RemoveStudentFromCourse(int courseId, int studentId);

    /// <summary>
    /// Retrieves the ID of the teacher assigned to a specific course.
    /// </summary>
    /// <param name="courseId">The ID of the course.</param>
    /// <returns>A task that represents the asynchronous operation. 
    /// The task result contains the teacher's user ID.</returns>
    public Task<int> GetTeacherId(int courseId);
}
