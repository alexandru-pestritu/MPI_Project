namespace DbProvider.Models;

public class CourseStudentLink
{
    public int Id { get; set; }
    
    public int CourseId { get; set; }
    
    public int StudentId { get; set; }
    
    public CourseStudentLink(int courseId, int studentId)
    {
        CourseId = courseId;
        StudentId = studentId;
    }
}