using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DbProvider.Database;
using DbProvider.Models;
using DbProvider.Providers;
using Moq;
using NUnit.Framework;

namespace DbProvider.Tests
{
    [TestFixture]
    public class GradeProviderTests
    {
        private Mock<IDbManager> _dbManagerMock;
        private GradeProvider _gradeProvider;

        [SetUp]
        public void SetUp()
        {
            _dbManagerMock = new Mock<IDbManager>(MockBehavior.Strict);
            _gradeProvider = new GradeProvider(_dbManagerMock.Object);
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
                .Setup(db => db.UpdateAsync(
                    "Grades",
                    new KeyValuePair<string, object>("Id", 1),
                    It.Is<KeyValuePair<string, object>>(k => (string)k.Key == "StudentId" && (int)k.Value == 20),
                    It.Is<KeyValuePair<string, object>>(k => (string)k.Key == "CourseId" && (int)k.Value == 10),
                    It.Is<KeyValuePair<string, object>>(k => (string)k.Key == "Value" && (int)k.Value == 8),
                    It.IsAny<KeyValuePair<string, object>>()))
                .ReturnsAsync(true);
            var result = await _gradeProvider.EditGrade(grade);
            Assert.That(result, Is.True);
            _dbManagerMock.VerifyAll();
        }

        [Test]
        public async Task EditGrade_UpdateFail_ReturnsFalse()
        {
            var grade = new Grade(1, 10, 20, 6, DateTime.Now);
            _dbManagerMock
                .Setup(db => db.UpdateAsync(
                    "Grades",
                    new KeyValuePair<string, object>("Id", 1),
                    It.IsAny<KeyValuePair<string, object>>(),
                    It.IsAny<KeyValuePair<string, object>>(),
                    It.IsAny<KeyValuePair<string, object>>(),
                    It.IsAny<KeyValuePair<string, object>>()))
                .ReturnsAsync(false);
            var result = await _gradeProvider.EditGrade(grade);
            Assert.That(result, Is.False);
            _dbManagerMock.VerifyAll();
        }

        [Test]
        public async Task AddGrades_SkipsInvalidValues()
        {
            var list = new List<Grade>
            {
                new Grade(0, 10, 20, 8, DateTime.Now),
                new Grade(0, 10, 21, 20, DateTime.Now)
            };
            _dbManagerMock
                .Setup(db => db.InsertAsyncWithReturn<int>(
                    "Grades",
                    "Id",
                    It.IsAny<KeyValuePair<string, object>[]>()))
                .ReturnsAsync(111);
            var result = await _gradeProvider.AddGrades(list);
            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result[0], Is.Not.Null);
            Assert.That(result[1], Is.Null);
            _dbManagerMock.Verify(
                db => db.InsertAsyncWithReturn<int>(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<KeyValuePair<string, object>[]>()), 
                Times.Once);
            _dbManagerMock.VerifyAll();
        }

        [Test]
        public async Task AddGrades_AllValid()
        {
            var list = new List<Grade>
            {
                new Grade(0, 10, 20, 8, DateTime.Now),
                new Grade(0, 10, 21, 9, DateTime.Now)
            };
            var seq = _dbManagerMock.SetupSequence(db => db.InsertAsyncWithReturn<int>(
                "Grades",
                "Id",
                It.IsAny<KeyValuePair<string, object>[]>()))
                .ReturnsAsync(100)
                .ReturnsAsync(101);
            var result = await _gradeProvider.AddGrades(list);
            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result[0]?.Id, Is.EqualTo(100));
            Assert.That(result[1]?.Id, Is.EqualTo(101));
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
    }
}
