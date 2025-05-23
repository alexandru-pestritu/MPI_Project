using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using DbProvider.Database;
using DbProvider.Models;
using DbProvider.Providers;
using Microsoft.AspNetCore.Http;
using Moq;
using NUnit.Framework;

namespace DbProvider.Tests
{
    [TestFixture]
    public class GradeProviderTests
    {
        private Mock<IDbManager> _dbManagerMock;
        private Mock<IUserProvider> _userProviderMock;
        private Mock<ICourseProvider> _courseProviderMock;
        private GradeProvider _gradeProvider;

        [SetUp]
        public void SetUp()
        {
            _dbManagerMock = new Mock<IDbManager>(MockBehavior.Strict);
            _userProviderMock = new Mock<IUserProvider>(MockBehavior.Strict);
            _courseProviderMock = new Mock<ICourseProvider>(MockBehavior.Strict);
            _gradeProvider = new GradeProvider(_dbManagerMock.Object, _userProviderMock.Object, _courseProviderMock.Object);
        }

        [Test]
        public async Task GetGrades_ReturnsGradesList()
        {
            var grades = new List<Grade>
            {
                new Grade(1, 10, 20, 5, DateTime.Now),
                new Grade(2, 10, 21, 7, DateTime.Now)
            };
            _dbManagerMock
                .Setup(db => db.ReadListOfTypeAsync(
                    "SELECT * FROM Grades WHERE CourseId = @CourseId",
                    It.IsAny<Func<object[], Grade>>(),
                    It.Is<KeyValuePair<string, object>[]>(p => (int)p[0].Value == 10)))
                .ReturnsAsync(grades);

            var result = await _gradeProvider.GetGrades(10);

            Assert.That(result.Count, Is.EqualTo(2));
            _dbManagerMock.VerifyAll();
        }

        [Test]
        public async Task GetGrades_ReturnsEmptyList()
        {
            _dbManagerMock
                .Setup(db => db.ReadListOfTypeAsync(
                    "SELECT * FROM Grades WHERE CourseId = @CourseId",
                    It.IsAny<Func<object[], Grade>>(),
                    It.IsAny<KeyValuePair<string, object>[]>()))
                .ReturnsAsync(new List<Grade>());

            var result = await _gradeProvider.GetGrades(999);

            Assert.That(result.Count, Is.EqualTo(0));
            _dbManagerMock.VerifyAll();
        }

        [Test]
        public async Task EditGrade_InvalidValue_ReturnsFalse()
        {
                        var grade = new Grade(1, 10, 20, 20, DateTime.Now);

            var result = await _gradeProvider.EditGrade(grade);

            Assert.That(result, Is.False);
        }

        [Test]
        public async Task EditGrade_UpdateSuccess_ReturnsTrue()
        {
            var grade = new Grade(1, 10, 20, 8, DateTime.Now);

                        _dbManagerMock
                .Setup(db => db.InsertAsync(
                    "GradeHistory",
                    new KeyValuePair<string, object>("GradeId", grade.Id),
                    new KeyValuePair<string, object>("Value", grade.Value),
                    new KeyValuePair<string, object>("Date", grade.Date)))
                .ReturnsAsync(true);

                        _dbManagerMock
                            .Setup(db => db.UpdateAsync(
                                "Grades",
                                new KeyValuePair<string, object>("Id", grade.Id),
                                It.IsAny<KeyValuePair<string, object>[]>()))
                            .ReturnsAsync(true);

            var result = await _gradeProvider.EditGrade(grade);

            Assert.That(result, Is.True);

                        _dbManagerMock.Verify(db => db.InsertAsync(
                    "GradeHistory",
                    It.IsAny<KeyValuePair<string, object>>(),
                    It.IsAny<KeyValuePair<string, object>>(),
                    It.IsAny<KeyValuePair<string, object>>()),
                Times.Once);

                        _dbManagerMock.Verify(db => db.UpdateAsync(
                                "Grades",
                                new KeyValuePair<string, object>("Id", grade.Id),
                                It.IsAny<KeyValuePair<string, object>[]>()), 
                            Times.Once);

            _dbManagerMock.VerifyAll();
        }

        [Test]
        public async Task EditGrade_UpdateFail_ReturnsFalse()
        {
            var grade = new Grade(1, 10, 20, 6, DateTime.Now);

                        _dbManagerMock
                .Setup(db => db.InsertAsync(
                    "GradeHistory",
                    new KeyValuePair<string, object>("GradeId", grade.Id),
                    new KeyValuePair<string, object>("Value", grade.Value),
                    new KeyValuePair<string, object>("Date", grade.Date)))
                .ReturnsAsync(true);

                        _dbManagerMock
                .Setup(db => db.UpdateAsync(
                    "Grades",
                    It.Is<KeyValuePair<string, object>>(pk => pk.Key == "Id" && (int)pk.Value == grade.Id),
                    It.IsAny<KeyValuePair<string, object>[]>()))
                .ReturnsAsync(false);

            var result = await _gradeProvider.EditGrade(grade);

            Assert.That(result, Is.False);

                        _dbManagerMock.Verify(db => db.InsertAsync(
                    "GradeHistory",
                    It.IsAny<KeyValuePair<string, object>>(),
                    It.IsAny<KeyValuePair<string, object>>(),
                    It.IsAny<KeyValuePair<string, object>>()),
                Times.Once);

                        _dbManagerMock.Verify(db => db.UpdateAsync(
                "Grades",
                It.IsAny<KeyValuePair<string, object>>(),                  It.IsAny<KeyValuePair<string, object>[]>()             ), Times.Once);

            _dbManagerMock.VerifyAll();
        }

        [Test]
        public async Task AddGrades_SkipsInvalidValues()
        {
            var now = DateTime.Now;
            var list = new List<Grade>
            {
                                new Grade(0, 10, 20, 8, now),
                                new Grade(0, 10, 21, 20, now)
            };

                        _dbManagerMock
                .Setup(db => db.InsertAsyncWithReturn<int>(
                    "Grades",
                    "Id",
                    It.IsAny<KeyValuePair<string, object>[]>()))
                .ReturnsAsync(111);

                        _dbManagerMock
                .Setup(db => db.InsertAsync(
                    "GradeHistory",
                    new KeyValuePair<string, object>("GradeId", 111),
                    new KeyValuePair<string, object>("Value", 8),
                    new KeyValuePair<string, object>("Date", now)))
                .ReturnsAsync(true);

            var result = await _gradeProvider.AddGrades(list);

                        Assert.That(result.Count, Is.EqualTo(2));
                        Assert.That(result[0], Is.Not.Null);
            Assert.That(result[0]?.Id, Is.EqualTo(111));
                        Assert.That(result[1], Is.Null);

                        _dbManagerMock.Verify(
                db => db.InsertAsyncWithReturn<int>(
                    "Grades",
                    "Id",
                    It.IsAny<KeyValuePair<string, object>[]>()),
                Times.Once);

                        _dbManagerMock.Verify(
                db => db.InsertAsync(
                    "GradeHistory",
                    It.Is<KeyValuePair<string, object>>(p => p.Key == "GradeId" && (int)p.Value == 111),
                    It.Is<KeyValuePair<string, object>>(p => p.Key == "Value" && (int)p.Value == 8),
                    It.Is<KeyValuePair<string, object>>(p => p.Key == "Date" && (DateTime)p.Value == now)),
                Times.Once);

            _dbManagerMock.VerifyAll();
        }

        [Test]
        public async Task AddGrades_AllValid()
        {
            var now = DateTime.Now;
            var list = new List<Grade>
            {
                new Grade(0, 10, 20, 8, now),
                new Grade(0, 10, 21, 9, now)
            };

                        var seqGrades = _dbManagerMock.SetupSequence(db => db.InsertAsyncWithReturn<int>(
                    "Grades",
                    "Id",
                    It.IsAny<KeyValuePair<string, object>[]>()))
                .ReturnsAsync(100)                 .ReturnsAsync(101); 
                        _dbManagerMock
                .Setup(db => db.InsertAsync(
                    "GradeHistory",
                    new KeyValuePair<string, object>("GradeId", 100),
                    new KeyValuePair<string, object>("Value", 8),
                    new KeyValuePair<string, object>("Date", now)))
                .ReturnsAsync(true);

            _dbManagerMock
                .Setup(db => db.InsertAsync(
                    "GradeHistory",
                    new KeyValuePair<string, object>("GradeId", 101),
                    new KeyValuePair<string, object>("Value", 9),
                    new KeyValuePair<string, object>("Date", now)))
                .ReturnsAsync(true);

            var result = await _gradeProvider.AddGrades(list);

            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result[0]?.Id, Is.EqualTo(100));
            Assert.That(result[1]?.Id, Is.EqualTo(101));

                        _dbManagerMock.Verify(
                db => db.InsertAsyncWithReturn<int>(
                    "Grades",
                    "Id",
                    It.IsAny<KeyValuePair<string, object>[]>()),
                Times.Exactly(2));

                        _dbManagerMock.Verify(
                db => db.InsertAsync(
                    "GradeHistory",
                    It.IsAny<KeyValuePair<string, object>>(),
                    It.IsAny<KeyValuePair<string, object>>(),
                    It.IsAny<KeyValuePair<string, object>>()),
                Times.Exactly(2));

            _dbManagerMock.VerifyAll();
        }

        [Test]
        public async Task DeleteGrade_Success()
        {
            _dbManagerMock
                .Setup(db => db.DeleteAsync("Grades", new KeyValuePair<string, object>("Id", 50)))
                .ReturnsAsync(true);

            var result = await _gradeProvider.DeleteGrade(50);

            Assert.That(result, Is.True);
            _dbManagerMock.VerifyAll();
        }

        [Test]
        public async Task DeleteGrade_Fail()
        {
            _dbManagerMock
                .Setup(db => db.DeleteAsync("Grades", new KeyValuePair<string, object>("Id", 99)))
                .ReturnsAsync(false);

            var result = await _gradeProvider.DeleteGrade(99);

            Assert.That(result, Is.False);
            _dbManagerMock.VerifyAll();
        }

        [Test]
        public async Task GetGradesByStudent_ReturnsList()
        {
            var list = new List<Grade>
            {
                new Grade(1, 10, 20, 8, DateTime.Now),
                new Grade(2, 11, 20, 9, DateTime.Now)
            };
            _dbManagerMock
                .Setup(db => db.ReadListOfTypeAsync(
                    "SELECT * FROM Grades WHERE StudentId = @StudentId",
                    It.IsAny<Func<object[], Grade>>(),
                    It.Is<KeyValuePair<string, object>[]>(p => (int)p[0].Value == 20)))
                .ReturnsAsync(list);

            var result = await _gradeProvider.GetGradesByStudent(20);

            Assert.That(result?.Count, Is.EqualTo(2));
            _dbManagerMock.VerifyAll();
        }

        [Test]
        public async Task GetGradesByStudent_EmptyList()
        {
            _dbManagerMock
                .Setup(db => db.ReadListOfTypeAsync(
                    "SELECT * FROM Grades WHERE StudentId = @StudentId",
                    It.IsAny<Func<object[], Grade>>(),
                    It.IsAny<KeyValuePair<string, object>[]>()))
                .ReturnsAsync(new List<Grade>());

            var result = await _gradeProvider.GetGradesByStudent(999);

            Assert.That(result?.Count, Is.EqualTo(0));
            _dbManagerMock.VerifyAll();
        }

        #region BulkUploadFromCsvAsync Tests

        [Test]
        public void BulkUploadFromCsvAsync_FileIsNull_ThrowsArgumentException()
        {
            Assert.ThrowsAsync<ArgumentException>(async () =>
                await _gradeProvider.BulkUploadFromCsvAsync(null)
            );
        }

        [Test]
        public void BulkUploadFromCsvAsync_FileIsEmpty_ThrowsArgumentException()
        {
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.Length).Returns(0);

            Assert.ThrowsAsync<ArgumentException>(async () =>
                await _gradeProvider.BulkUploadFromCsvAsync(fileMock.Object)
            );
        }

        [Test]
        public async Task BulkUploadFromCsvAsync_SingleValidLine_InsertsGrade()
        {
            var csvContent = "10,1,8,2025-01-20\n";
            var fileMock = CreateMockFormFile(csvContent);

            _userProviderMock
                .Setup(u => u.getUserByIdAsync(1))
                .ReturnsAsync(new User(1, "stud1", "pass", "email", 0, true)); 
            _courseProviderMock
                .Setup(c => c.GetCourseById(10))
                .ReturnsAsync(new Course(10, 999, "CourseName", "Desc"));

                        _dbManagerMock
                .Setup(db => db.InsertAsyncWithReturn<int>(
                    "Grades",
                    "Id",
                    It.IsAny<KeyValuePair<string, object>[]>()))
                .ReturnsAsync(123);

                        _dbManagerMock
                .Setup(db => db.InsertAsync(
                    "GradeHistory",
                    new KeyValuePair<string, object>("GradeId", 123),
                    new KeyValuePair<string, object>("Value", 8),
                    It.IsAny<KeyValuePair<string, object>>()                 ))
                .ReturnsAsync(true);

            var result = await _gradeProvider.BulkUploadFromCsvAsync(fileMock.Object);

            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0], Is.Not.Null);
            Assert.That(result[0]?.Id, Is.EqualTo(123));
            Assert.That(result[0]?.CourseId, Is.EqualTo(1));
            Assert.That(result[0]?.StudentId, Is.EqualTo(10));
            Assert.That(result[0]?.Value, Is.EqualTo(8));

                        _dbManagerMock.Verify(db => db.InsertAsyncWithReturn<int>(
                "Grades",
                "Id",
                It.IsAny<KeyValuePair<string, object>[]>()), Times.Once);

            _dbManagerMock.Verify(db => db.InsertAsync(
                "GradeHistory",
                It.Is<KeyValuePair<string, object>>(p => p.Key == "GradeId" && (int)p.Value == 123),
                It.Is<KeyValuePair<string, object>>(p => p.Key == "Value" && (int)p.Value == 8),
                It.IsAny<KeyValuePair<string, object>>()), Times.Once);

            _userProviderMock.VerifyAll();
            _courseProviderMock.VerifyAll();
            _dbManagerMock.VerifyAll();
        }

        [Test]
        public async Task BulkUploadFromCsvAsync_MultipleLines_SomeFailParsing_SomeFailValidation()
        {
            var csvContent = new StringBuilder()
                .AppendLine("10,1,8,2025-01-20")                   .AppendLine("NOT_ENOUGH_COLUMNS")                 .AppendLine("10,2,9,BADDATE")                     .AppendLine("10,3,8,2025-03-15")                 .AppendLine("999,4,8,2025-04-01")                .AppendLine("10,5,15,2025-05-10")                .AppendLine("10,6,9,2025-06-01")                 .ToString();

            var fileMock = CreateMockFormFile(csvContent);

                        _courseProviderMock
                .Setup(c => c.GetCourseById(10))
                .ReturnsAsync(new Course(10, 999, "CourseName", "Desc"));
            _userProviderMock
                .Setup(u => u.getUserByIdAsync(1))
                .ReturnsAsync(new User(1, "stud1", "pass", "email", 0, true));

                        _userProviderMock
                .Setup(u => u.getUserByIdAsync(3))
                .ReturnsAsync(new User(3, "teacher1", "pass", "email", 1, true));

                        _courseProviderMock
                .Setup(c => c.GetCourseById(999))
                .ReturnsAsync((Course?)null);
            _userProviderMock
                .Setup(u => u.getUserByIdAsync(4))
                .ReturnsAsync(new User(4, "stud4", "pass", "email", 0, true));

                        _userProviderMock
                .Setup(u => u.getUserByIdAsync(5))
                .ReturnsAsync(new User(5, "stud5", "pass", "email", 0, true));

                        _userProviderMock
                .Setup(u => u.getUserByIdAsync(6))
                .ReturnsAsync(new User(6, "stud6", "pass", "email", 0, true));

                        _dbManagerMock
                .SetupSequence(db => db.InsertAsyncWithReturn<int>(
                    "Grades",
                    "Id",
                    It.IsAny<KeyValuePair<string, object>[]>()))
                .ReturnsAsync(101)                 .ReturnsAsync(102)                 ;

                        _dbManagerMock
                .Setup(db => db.InsertAsync(
                    "GradeHistory",
                    new KeyValuePair<string, object>("GradeId", 101),
                    new KeyValuePair<string, object>("Value", 8),
                    It.IsAny<KeyValuePair<string, object>>()))
                .ReturnsAsync(true);

            _dbManagerMock
                .Setup(db => db.InsertAsync(
                    "GradeHistory",
                    new KeyValuePair<string, object>("GradeId", 102),
                    new KeyValuePair<string, object>("Value", 9),
                    It.IsAny<KeyValuePair<string, object>>()))
                .ReturnsAsync(true);

            var result = await _gradeProvider.BulkUploadFromCsvAsync(fileMock.Object);

                        Assert.That(result.Count, Is.EqualTo(7));

                        Assert.That(result[0], Is.Not.Null);
            Assert.That(result[0]?.Id, Is.EqualTo(101));

                        Assert.That(result[1], Is.Null);

                        Assert.That(result[2], Is.Null);

                        Assert.That(result[3], Is.Null);

                        Assert.That(result[4], Is.Null);

                        Assert.That(result[5], Is.Null);

                        Assert.That(result[6], Is.Not.Null);
            Assert.That(result[6]?.Id, Is.EqualTo(102));

                        _dbManagerMock.Verify(db => db.InsertAsyncWithReturn<int>(
                "Grades",
                "Id",
                It.IsAny<KeyValuePair<string, object>[]>()),
                Times.Exactly(2));

                        _dbManagerMock.Verify(db => db.InsertAsync(
                "GradeHistory",
                It.IsAny<KeyValuePair<string, object>>(),
                It.IsAny<KeyValuePair<string, object>>(),
                It.IsAny<KeyValuePair<string, object>>()),
                Times.Exactly(2));

            _userProviderMock.VerifyAll();
            _courseProviderMock.VerifyAll();
            _dbManagerMock.VerifyAll();
        }

        #endregion

        [Test]
        public async Task GetStudentGradesAtCourse_ReturnsList()
        {
            var grades = new List<Grade>
            {
                new Grade(1, 10, 20, 8, DateTime.Now),
                new Grade(2, 10, 20, 9, DateTime.Now)
            };
    
            _dbManagerMock
                .Setup(db => db.ReadListOfTypeAsync(
                    "SELECT * FROM Grades WHERE StudentId = @StudentId AND CourseId = @CourseId",
                    It.IsAny<Func<object[], Grade>>(),
                    It.Is<KeyValuePair<string, object>[]>(p => 
                        (int)p[0].Value == 10 && (int)p[1].Value == 20)))
                .ReturnsAsync(grades);
    
            var result = await _gradeProvider.GetStudentGradesAtCourse(10, 20);
    
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(2));
            _dbManagerMock.VerifyAll();
        }

        #region GetStudentGradesAtCourse Tests

        [Test]
        public async Task GetStudentGradesAtCourse_ReturnsEmptyList()
        {
            _dbManagerMock
                .Setup(db => db.ReadListOfTypeAsync(
                    "SELECT * FROM Grades WHERE StudentId = @StudentId AND CourseId = @CourseId",
                    It.IsAny<Func<object[], Grade>>(),
                    It.IsAny<KeyValuePair<string, object>[]>()))
                .ReturnsAsync(new List<Grade>());

            var result = await _gradeProvider.GetStudentGradesAtCourse(999, 999);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(0));
            _dbManagerMock.VerifyAll();
        }

        #endregion

        #region GetAverageGrade Tests

        [Test]
        public async Task GetAverageGrade_NoGradesFound_ReturnsZero()
        {
            _dbManagerMock
                .Setup(db => db.ReadListOfTypeAsync(
                    "SELECT * FROM Grades WHERE StudentId = @StudentId",
                    It.IsAny<Func<object[], Grade>>(),
                    It.Is<KeyValuePair<string, object>[]>(p => (int)p[0].Value == 123)))
                .ReturnsAsync(new List<Grade>());

            float result = await _gradeProvider.GetAverageGrade(123);

            Assert.That(result, Is.EqualTo(0));
            _dbManagerMock.VerifyAll();
        }

        [Test]
        public async Task GetAverageGrade_GradesFound_ReturnsCorrectAverage()
        {
            var grades = new List<Grade>
            {
                new Grade(1, 10, 123, 8, DateTime.Now),
                new Grade(2, 10, 123, 9, DateTime.Now)
            };
            _dbManagerMock
                .Setup(db => db.ReadListOfTypeAsync(
                    "SELECT * FROM Grades WHERE StudentId = @StudentId",
                    It.IsAny<Func<object[], Grade>>(),
                    It.Is<KeyValuePair<string, object>[]>(p => (int)p[0].Value == 123)))
                .ReturnsAsync(grades);

            float result = await _gradeProvider.GetAverageGrade(123);

            Assert.That(result, Is.EqualTo(8.5f));
            _dbManagerMock.VerifyAll();
        }

        #endregion

        #region GetGradeHistory Tests

        [Test]
        public async Task GetGradeHistory_ReturnsEmptyList()
        {
            int testGradeId = 999;
            _dbManagerMock
                .Setup(db => db.ReadListOfTypeAsync(
                    "SELECT * FROM GradeHistory WHERE GradeId = @GradeId",
                    It.IsAny<Func<object[], GradeHistory>>(),
                    It.Is<KeyValuePair<string, object>[]>(p => (int)p[0].Value == testGradeId)))
                .ReturnsAsync(new List<GradeHistory>());

            var result = await _gradeProvider.GetGradeHistory(testGradeId);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(0));
            _dbManagerMock.VerifyAll();
        }

        [Test]
        public async Task GetGradeHistory_ReturnsNonEmptyList()
        {
            int testGradeId = 100;
            var historyList = new List<GradeHistory>
            {
                new GradeHistory(1, testGradeId, 8, DateTime.Now.AddDays(-2)),
                new GradeHistory(2, testGradeId, 9, DateTime.Now.AddDays(-1))
            };

            _dbManagerMock
                .Setup(db => db.ReadListOfTypeAsync(
                    "SELECT * FROM GradeHistory WHERE GradeId = @GradeId",
                    It.IsAny<Func<object[], GradeHistory>>(),
                    It.Is<KeyValuePair<string, object>[]>(p => (int)p[0].Value == testGradeId)))
                .ReturnsAsync(historyList);

            var result = await _gradeProvider.GetGradeHistory(testGradeId);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result[0].Id, Is.EqualTo(1));
            Assert.That(result[0].GradeId, Is.EqualTo(100));
            Assert.That(result[0].Value, Is.EqualTo(8));
            Assert.That(result[1].Id, Is.EqualTo(2));
            Assert.That(result[1].GradeId, Is.EqualTo(100));
            Assert.That(result[1].Value, Is.EqualTo(9));
            _dbManagerMock.VerifyAll();
        }

        #endregion

        #region Helpers

                                                private static Mock<IFormFile> CreateMockFormFile(string fileContent)
        {
            var fileMock = new Mock<IFormFile>();

            var contentBytes = Encoding.UTF8.GetBytes(fileContent);
            var memoryStream = new MemoryStream(contentBytes);

            fileMock.Setup(f => f.OpenReadStream()).Returns(memoryStream);
            fileMock.Setup(f => f.Length).Returns(contentBytes.Length);

            return fileMock;
        }

        #endregion
    }
}
