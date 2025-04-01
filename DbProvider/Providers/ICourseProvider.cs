using DbProvider.Models;
using DbProvider.Models.ProviderOutputs;

namespace DbProvider.Providers;

public interface ICourseProvider
{
    public Task<List<Course>> GetCourses(User user);
    
    
    public Task<Course?> GetCourseById(int courseId);
    public Task<Course> AddCourse(Course course);
    public Task<BaseResponse> EditCourse(Course course);
    public Task<BaseResponse> DeleteCourse(int courseId);
    
    public Task<BaseResponse> AddStudentToCourse(int courseId, int studentId);
    
    public Task<BaseResponse> RemoveStudentFromCourse(int courseId, int studentId);
    
    public Task<int> GetTeacherId(int courseId);
}