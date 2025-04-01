namespace backend.Models;

public class RemoveStudentFromCourseRequest
{
    public int CourseId { get; set; }
    public List<int> StudentIds { get; set; }
    
    public RemoveStudentFromCourseRequest(int courseId, List<int> studentIds)
    {
        CourseId = courseId;
        StudentIds = studentIds;
    }
    
}