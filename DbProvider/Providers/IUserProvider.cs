using DbProvider.Models;

namespace DbProvider.Providers;

/// <summary>
/// Defines methods for accessing and managing user and user profile data.
/// </summary>
public interface IUserProvider
{
    /// <summary>
    /// Retrieves the profile of a user by their user ID.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <returns>A task that represents the asynchronous operation. 
    /// The task result contains a <see cref="UserProfile"/> if found; otherwise, null.</returns>
    public Task<UserProfile?> getUserProfileAsync(int userId);

    /// <summary>
    /// Updates the profile of a user.
    /// </summary>
    /// <param name="profile">The updated user profile data.</param>
    /// <returns>A task that represents the asynchronous operation. 
    /// The task result contains the updated <see cref="UserProfile"/>, or null if the update failed.</returns>
    public Task<UserProfile?> updateUserProfileAsync(UserProfile profile);

    /// <summary>
    /// Retrieves a user by their user ID.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <returns>A task that represents the asynchronous operation. 
    /// The task result contains a <see cref="User"/> if found; otherwise, null.</returns>
    public Task<User?> getUserByIdAsync(int userId);

    /// <summary>
    /// Retrieves a user by their email address.
    /// </summary>
    /// <param name="email">The email address of the user.</param>
    /// <returns>A task that represents the asynchronous operation. 
    /// The task result contains a <see cref="User"/> if found; otherwise, null.</returns>
    public Task<User?> getUserByEmailAsync(string email);

    /// <summary>
    /// Retrieves a list of student profiles enrolled in a specific course.
    /// </summary>
    /// <param name="courseId">The unique identifier of the course.</param>
    /// <returns>A task that represents the asynchronous operation. 
    /// The task result contains a list of <see cref="UserProfile"/> representing the students.</returns>
    public Task<List<UserProfile>> GetStudentsInCourse(int courseId);

    /// <summary>
    /// Retrieves a list of all student profiles.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. 
    /// The task result contains a list of all <see cref="UserProfile"/> objects classified as students.</returns>
    public Task<List<UserProfile>> GetAllStudents();
}
