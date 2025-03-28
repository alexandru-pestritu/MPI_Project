namespace DbProvider.Models;

public class Grade
{
    public int Id { get; set; }
    
    public int CourseId { get; set; }

    public int StudentId { get; set; }
    
    public int Value { get; set; }
    
    public DateTime Date { get; set; }
    
    public Grade(int courseId, int studentId, int value, DateTime date)
    {
        CourseId = courseId;
        StudentId = studentId;
        Value = value;
        Date = date;
    }
}