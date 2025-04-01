namespace backend.Models;

public class AddStudentToCourseRequest
{
    public int CourseId { get; set; }
    public List<int> StudentIds { get; set; }
    
    public AddStudentToCourseRequest(int courseId, List<int> studentIds)
    {
        CourseId = courseId;
        StudentIds = studentIds;
    }
}