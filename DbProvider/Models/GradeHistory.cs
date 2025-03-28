namespace DbProvider.Models;

public class GradeHistory
{
    public int Id { get; set; }
    
    public int GradeId { get; set; }
    
    public int Value { get; set; }
    
    public DateTime Date { get; set; }
    
    public GradeHistory(int gradeId, int value, DateTime date)
    {
        GradeId = gradeId;
        Value = value;
        Date = date;
    }
}