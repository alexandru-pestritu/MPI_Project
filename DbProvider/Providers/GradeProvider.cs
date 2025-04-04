using System.Globalization;
using DbProvider.Database;
using DbProvider.Models;
using Microsoft.AspNetCore.Http;

namespace DbProvider.Providers;

public class GradeProvider : IGradeProvider
{
    
    private readonly IDbManager _manager;
    private readonly IUserProvider _userProvider;
    private readonly ICourseProvider _courseProvider;
    
    public GradeProvider(IDbManager manager, IUserProvider userProvider, ICourseProvider courseProvider)
    {
        _manager = manager;
        _userProvider = userProvider;
        _courseProvider = courseProvider;
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
        
        await _manager.InsertAsync("GradeHistory", new KeyValuePair<string, object>("GradeId", grade.Id),
            new KeyValuePair<string, object>("Value", grade.Value),
            new KeyValuePair<string, object>("Date", grade.Date));
        
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
            
            await _manager.InsertAsync("GradeHistory", new KeyValuePair<string, object>("GradeId", id),
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
    
    public async Task<List<Grade?>> BulkUploadFromCsvAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File cannot be null or empty.", nameof(file));

            var validGradesToAdd = new List<Grade>();
            var resultList = new List<Grade?>(); 
           
            try
            {
                using var streamReader = new StreamReader(file.OpenReadStream());
                
                

                string? line;
                while ((line = await streamReader.ReadLineAsync()) != null)
                {
                    var columns = line.Split(',');
                   

                    if (columns.Length < 4)
                    {
                        
                        resultList.Add(null);
                        continue;
                    }

                   
                    if (!int.TryParse(columns[1], out var studentId))
                    {
                        resultList.Add(null);
                        continue;
                    }

                    if (!int.TryParse(columns[0], out var courseId))
                    {
                        resultList.Add(null);
                        continue;
                    }

                    if (!int.TryParse(columns[2], out var value))
                    {
                        resultList.Add(null);
                        continue;
                    }

                    if (!DateTime.TryParse(
                            columns[3],
                            CultureInfo.InvariantCulture,
                            DateTimeStyles.None,
                            out var date
                        ))
                    {
                        resultList.Add(null);
                        continue;
                    }

                    
                    var user = await _userProvider.getUserByIdAsync(studentId);
                    if (user == null || user.Role != 0) 
                    {
                       
                        resultList.Add(null);
                        continue;
                    }

                    
                    var course = await _courseProvider.GetCourseById(courseId);
                    if (course == null)
                    {
                        
                        resultList.Add(null);
                        continue;
                    }

                  
                    if (!IsGradeValueValid(value))
                    {
                        resultList.Add(null);
                        continue;
                    }

                    
                    var validGrade = new Grade(0, courseId, studentId, value, date);
                    
                   
                    validGradesToAdd.Add(validGrade);
                   
                    resultList.Add(validGrade);
                }
            }
            catch (Exception ex)
            {
               
                throw new IOException($"Error while reading CSV file: {ex.Message}", ex);
            }

           
            var insertedGrades = await AddGrades(validGradesToAdd);

            
            int insertIndex = 0; 
            for (int i = 0; i < resultList.Count; i++)
            {
                if (resultList[i] != null)
                {
                    
                    resultList[i] = insertedGrades[insertIndex];
                    insertIndex++;
                }
            }

            return resultList;
        }

    public async Task<List<Grade>> GetStudentGradesAtCourse(int studentId, int courseId)
    {
        string query = "SELECT * FROM Grades WHERE StudentId = @StudentId AND CourseId = @CourseId";
        return await _manager.ReadListOfTypeAsync(query, ConvertGrade,
            new KeyValuePair<string, object>("StudentId", studentId),
            new KeyValuePair<string, object>("CourseId", courseId));
    }

    public async Task<float> GetAverageGrade(int studentId)
    {
        string query = "SELECT * FROM Grades WHERE StudentId = @StudentId";
        var grades = await _manager.ReadListOfTypeAsync(query, ConvertGrade, new KeyValuePair<string, object>("StudentId", studentId));
        if (grades.Count == 0)
            return 0;
        
        float sum = 0;
        foreach (var grade in grades)
        {
            sum += grade.Value;
        }
        return sum / grades.Count;
    }

    public async Task<List<GradeHistory>> GetGradeHistory(int gradeId)
    {
        string query = "SELECT * FROM GradeHistory WHERE GradeId = @GradeId";
        return await _manager.ReadListOfTypeAsync(query, ConvertGradeHistory, new KeyValuePair<string, object>("GradeId", gradeId));
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

    private GradeHistory ConvertGradeHistory(object[] values)
    {
        int id = (int)values[0];
        int gradeId = (int)values[1];
        int value = (int)values[2];
        System.DateTime date = (System.DateTime)values[3];
        return new GradeHistory(id,gradeId, value, date);
    }
}