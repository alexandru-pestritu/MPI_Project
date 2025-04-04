namespace DbProvider.Models;

public class GradeHistory
{
    public int Id { get; set; }
    
    public int GradeId { get; set; }
    
    public int Value { get; set; }
    
    public DateTime Date { get; set; }
    
    public GradeHistory(int id,int gradeId, int value, DateTime date)
    {
        Id = id;
        GradeId = gradeId;
        Value = value;
        Date = date;
    }
}