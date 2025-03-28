namespace DbProvider.Models;

public class Course
{
    public int Id { get; set; }
    
    public int TeacherId { get; set; }
    
    public string Name { get; set; }
    
    public string Description { get; set; }
    
    public Course(int id, int teacherId, string name, string description)
    {
        Id = id;
        TeacherId = teacherId;
        Name = name;
        Description = description;
    }
}