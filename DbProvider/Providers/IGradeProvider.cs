using DbProvider.Models;

namespace DbProvider.Providers;

public interface IGradeProvider
{
    public Task<List<Grade>> GetGrades(int courseId);
    
    public Task<bool> EditGrade(Grade grade);
    
    public Task<List<Grade?>> AddGrades(List<Grade> grades);
    
    public Task<bool> DeleteGrade(int gradeId);
    
    public Task<List<Grade>?> GetGradesByStudent(int studentId);
}