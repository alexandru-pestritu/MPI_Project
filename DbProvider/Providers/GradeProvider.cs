using DbProvider.Database;
using DbProvider.Models;

namespace DbProvider.Providers;

public class GradeProvider : IGradeProvider
{
    
    private readonly IDbManager _manager;
    
    public GradeProvider(IDbManager manager)
    {
        _manager = manager;
    }
    
    
    public async Task<List<Grade>> GetGrades(int courseId)
    {
        string query = "SELECT * FROM Grades WHERE CourseId = @CourseId";
        return await _manager.ReadListOfTypeAsync(query, ConvertGrade, new KeyValuePair<string, object>("CourseId", courseId));
    }

    public async Task<bool> EditGrade(Grade grade)
    {
        
        if(!IsGradeValueValid(grade.Value))
            return false;
        
        return await _manager.UpdateAsync("Grades",
            new KeyValuePair<string, object>("Id", grade.Id),
            new KeyValuePair<string, object>("StudentId", grade.StudentId),
            new KeyValuePair<string, object>("CourseId", grade.CourseId),
            new KeyValuePair<string, object>("Value", grade.Value),
            new KeyValuePair<string, object>("Date", grade.Date));
    }

    public async Task<List<Grade?>> AddGrades(List<Grade> grades)
    {
        List<Grade?> result = new List<Grade?>();
        foreach (var grade in grades)
        {
            if (!IsGradeValueValid(grade.Value))
            {
                result.Add(null);
                continue;
            }
            int id = await _manager.InsertAsyncWithReturn<int>("Grades", "Id",
                new KeyValuePair<string, object>("StudentId", grade.StudentId),
                new KeyValuePair<string, object>("CourseId", grade.CourseId),
                new KeyValuePair<string, object>("Value", grade.Value),
                new KeyValuePair<string, object>("Date", grade.Date));
            
            result.Add(new Grade(id,grade.StudentId,grade.CourseId,grade.Value,grade.Date));
        }

        return result;
    }

    public async Task<bool> DeleteGrade(int gradeId)
    {
        return await _manager.DeleteAsync("Grades", new KeyValuePair<string, object>("Id", gradeId));
    }

    public async Task<List<Grade>?> GetGradesByStudent(int studentId)
    {
        string query = "SELECT * FROM Grades WHERE StudentId = @StudentId";
        return await _manager.ReadListOfTypeAsync(query, ConvertGrade, new KeyValuePair<string, object>("StudentId", studentId));
    }

    private bool IsGradeValueValid(int value)
    {
        return value >= 1 && value <= 10;
    }
    
    private Grade ConvertGrade(object[] values)
    {
        int id = (int)values[0];
        int studentId = (int)values[1];
        int courseId = (int)values[2];
        int value = (int)values[3];
        System.DateTime date = (System.DateTime)values[4];
        
        return new Grade(id,studentId, courseId, value, date);
        
    }
}