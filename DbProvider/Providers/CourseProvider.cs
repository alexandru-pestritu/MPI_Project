using DbProvider.Database;
using DbProvider.Models;
using DbProvider.Models.ProviderOutputs;

namespace DbProvider.Providers;

public class CourseProvider : ICourseProvider
{
    
    private readonly IDbManager _manager;

    public CourseProvider(IDbManager manager)
    {
        _manager = manager;
    }
    
    public async Task<List<Course>> GetCourses(User user)
    {
        string query = String.Empty;
        
        if (user.Role == 0)
        {
            query = "SELECT * FROM Courses join CourseStudentLink on Courses.Id = CourseStudentLink.CourseId WHERE CourseStudentLink.StudentId = @Id";
            return await _manager.ReadListOfTypeAsync(query,ConvertCourse, new KeyValuePair<string, object>("Id", user.Id));
        }
        
        query = "SELECT * FROM Courses WHERE TeacherId = @Id";
        
        return await _manager.ReadListOfTypeAsync(query,ConvertCourse, new KeyValuePair<string, object>("Id", user.Id));
    }

    public async Task<Course> AddCourse(Course course)
    {
        course.Id= await _manager.InsertAsyncWithReturn<int>("Courses", "Id",
            new KeyValuePair<string, object>("TeacherId", course.TeacherId),
            new KeyValuePair<string, object>("Name", course.Name),
            new KeyValuePair<string, object>("Description", course.Description));

        return course;
    }

    public async Task<BaseResponse> EditCourse(Course course)
    {
        bool res = await _manager.UpdateAsync("Courses", new KeyValuePair<string, object>("Id", course.Id),
            new KeyValuePair<string, object>("TeacherId", course.TeacherId),
            new KeyValuePair<string, object>("Name", course.Name),
            new KeyValuePair<string, object>("Description", course.Description));

        return res;
    }

    public async Task<BaseResponse> DeleteCourse(int courseId)
    {
        await _manager.DeleteAsync("CourseStudentLink", new KeyValuePair<string, object>("CourseId", courseId));
        
        return await _manager.DeleteAsync("Courses", new KeyValuePair<string, object>("Id", courseId));
    }

    public async Task<BaseResponse> AddStudentToCourse(int courseId, int studentId)
    {
        string query = "SELECT Id FROM CourseStudentLink WHERE CourseId = @CourseId AND StudentId = @StudentId";
        
        object[]? id = await _manager.ReadDataArrayAsync(query,
            new KeyValuePair<string, object>("CourseId", courseId),
            new KeyValuePair<string, object>("StudentId", studentId));
        
        if(id is not null)
            return "Student already in course";
        
        return await _manager.InsertAsync("CourseStudentLink", new KeyValuePair<string, object>("CourseId", courseId),
            new KeyValuePair<string, object>("StudentId", studentId));
    }

    public async Task<BaseResponse> RemoveStudentFromCourse(int courseId, int studentId)
    {
        return await _manager.DeleteAsync("CourseStudentLink", new KeyValuePair<string, object>("CourseId", courseId),
            new KeyValuePair<string, object>("StudentId", studentId));
    }

    public async Task<int> GetTeacherId(int courseId)
    {
        string query = "SELECT TeacherId FROM Courses WHERE Id = @Id";
        return await _manager.ReadObjectOfTypeAsync(query,(arr)=> (int)arr[0], new KeyValuePair<string, object>("Id", courseId));
    }


    private Course ConvertCourse(object[] values)
    {
        int id = (int)values[0];
        int teacherId = (int)values[1];
        string name = (string)values[2];
        string description = (string)values[3];
        
        return new Course(id, teacherId, name, description);
    }
   
}