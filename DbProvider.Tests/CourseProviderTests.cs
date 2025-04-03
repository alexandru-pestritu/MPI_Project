using System.Collections.Generic;
using System.Threading.Tasks;
using DbProvider.Database;
using DbProvider.Models;
using DbProvider.Models.ProviderOutputs;
using DbProvider.Providers;
using Moq;
using NUnit.Framework;

namespace DbProvider.Tests
{
    [TestFixture]
    public class CourseProviderTests
    {
        private Mock<IDbManager> _dbManagerMock;
        private CourseProvider _courseProvider;

        [SetUp]
        public void SetUp()
        {
            _dbManagerMock = new Mock<IDbManager>(MockBehavior.Strict);
            _courseProvider = new CourseProvider(_dbManagerMock.Object);
        }

        [Test]
        public async Task GetCourses_StudentRole_ReturnsStudentCourses()
        {
            var user = new User(1, "stud", "pass", "stud@example.com", 0, true);
            var courses = new List<Course> { new Course(10, 2, "Course1", "Desc1"), new Course(11, 2, "Course2", "Desc2") };
            _dbManagerMock
                .Setup(db => db.ReadListOfTypeAsync(
                    It.Is<string>(s => s.Contains("CourseStudentLink")),
                    It.IsAny<Func<object[], Course>>(),
                    It.Is<KeyValuePair<string, object>[]>(p => (int)p[0].Value == user.Id)))
                .ReturnsAsync(courses);
            var result = await _courseProvider.GetCourses(user);
            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result[0].Id, Is.EqualTo(10));
            Assert.That(result[1].Id, Is.EqualTo(11));
            _dbManagerMock.VerifyAll();
        }

        [Test]
        public async Task GetCourses_TeacherRole_ReturnsTeacherCourses()
        {
            var user = new User(2, "teach", "pass", "teach@example.com", 1, true);
            var courses = new List<Course> { new Course(50, 2, "TeachCourse", "TeachDesc") };
            _dbManagerMock
                .Setup(db => db.ReadListOfTypeAsync(
                    It.Is<string>(s => s.Contains("WHERE TeacherId = @Id")),
                    It.IsAny<Func<object[], Course>>(),
                    It.Is<KeyValuePair<string, object>[]>(p => (int)p[0].Value == user.Id)))
                .ReturnsAsync(courses);
            var result = await _courseProvider.GetCourses(user);
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].Id, Is.EqualTo(50));
            _dbManagerMock.VerifyAll();
        }

        [Test]
        public async Task GetCourseById_NotFound_ReturnsNull()
        {
            _dbManagerMock
                .Setup(db => db.ReadObjectOfTypeAsync(
                    "SELECT * FROM Courses WHERE Id = @Id",
                    It.IsAny<Func<object[], Course>>(),
                    It.Is<KeyValuePair<string, object>[]>(p => (int)p[0].Value == 100)))
                .ReturnsAsync((Course?)null);
            var result = await _courseProvider.GetCourseById(100);
            Assert.That(result, Is.Null);
            _dbManagerMock.VerifyAll();
        }

        [Test]
        public async Task GetCourseById_Found_ReturnsCourse()
        {
            var course = new Course(100, 5, "TestCourse", "Desc");
            _dbManagerMock
                .Setup(db => db.ReadObjectOfTypeAsync(
                    "SELECT * FROM Courses WHERE Id = @Id",
                    It.IsAny<Func<object[], Course>>(),
                    It.Is<KeyValuePair<string, object>[]>(p => (int)p[0].Value == 100)))
                .ReturnsAsync(course);
            var result = await _courseProvider.GetCourseById(100);
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(100));
            _dbManagerMock.VerifyAll();
        }

        [Test]
        public async Task AddCourse_AssignsIdAndReturnsCourse()
        {
            var course = new Course(0, 5, "NewCourse", "NewDesc");
            _dbManagerMock
                .Setup(db => db.InsertAsyncWithReturn<int>(
                    "Courses",
                    "Id",
                    It.IsAny<KeyValuePair<string, object>[]>()))
                .ReturnsAsync(10);
            var result = await _courseProvider.AddCourse(course);
            Assert.That(result.Id, Is.EqualTo(10));
            Assert.That(result.TeacherId, Is.EqualTo(5));
            _dbManagerMock.VerifyAll();
        }

        [Test]
        public async Task EditCourse_Success_ReturnsTrue()
        {
            var course = new Course(10, 5, "EditName", "EditDesc");
            _dbManagerMock
                .Setup(db => db.UpdateAsync(
                    "Courses",
                    new KeyValuePair<string, object>("Id", 10),
                    It.IsAny<KeyValuePair<string, object>>(),
                    It.IsAny<KeyValuePair<string, object>>(),
                    It.IsAny<KeyValuePair<string, object>>()))
                .ReturnsAsync(true);
            var result = await _courseProvider.EditCourse(course);
            Assert.That(result.IsSuccess, Is.True);
            _dbManagerMock.VerifyAll();
        }

        [Test]
        public async Task EditCourse_Failure_ReturnsFalse()
        {
            var course = new Course(10, 5, "EditName", "EditDesc");
            _dbManagerMock
                .Setup(db => db.UpdateAsync(
                    "Courses",
                    new KeyValuePair<string, object>("Id", 10),
                    It.IsAny<KeyValuePair<string, object>>(),
                    It.IsAny<KeyValuePair<string, object>>(),
                    It.IsAny<KeyValuePair<string, object>>()))
                .ReturnsAsync(false);
            var result = await _courseProvider.EditCourse(course);
            Assert.That(result.IsSuccess, Is.False);
            _dbManagerMock.VerifyAll();
        }

        [Test]
        public async Task DeleteCourse_DeletesLinksAndCourse_ReturnsResult()
        {
            _dbManagerMock
                .Setup(db => db.DeleteAsync(
                    "CourseStudentLink",
                    new KeyValuePair<string, object>("CourseId", 10)))
                .ReturnsAsync(true);
            _dbManagerMock
                .Setup(db => db.DeleteAsync(
                    "Courses",
                    new KeyValuePair<string, object>("Id", 10)))
                .ReturnsAsync(true);
            var result = await _courseProvider.DeleteCourse(10);
            Assert.That(result.IsSuccess, Is.True);
            _dbManagerMock.VerifyAll();
        }

        [Test]
        public async Task AddStudentToCourse_AlreadyInCourse_ReturnsErrorMessage()
        {
            _dbManagerMock
                .Setup(db => db.ReadDataArrayAsync(
                    "SELECT Id FROM CourseStudentLink WHERE CourseId = @CourseId AND StudentId = @StudentId",
                    new KeyValuePair<string, object>("CourseId", 10),
                    new KeyValuePair<string, object>("StudentId", 5)))
                .ReturnsAsync(new object[] { 1 });
            var result = await _courseProvider.AddStudentToCourse(10, 5);
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Message, Is.EqualTo("Student already in course"));
            _dbManagerMock.VerifyAll();
        }

        [Test]
        public async Task AddStudentToCourse_NotInCourse_InsertsLink()
        {
            _dbManagerMock
                .Setup(db => db.ReadDataArrayAsync(
                    "SELECT Id FROM CourseStudentLink WHERE CourseId = @CourseId AND StudentId = @StudentId",
                    new KeyValuePair<string, object>("CourseId", 10),
                    new KeyValuePair<string, object>("StudentId", 5)))
                .ReturnsAsync((object[]?)null);
            _dbManagerMock
                .Setup(db => db.InsertAsync(
                    "CourseStudentLink",
                    It.IsAny<KeyValuePair<string, object>[]>()))
                .ReturnsAsync(true);
            var result = await _courseProvider.AddStudentToCourse(10, 5);
            Assert.That(result.IsSuccess, Is.True);
            _dbManagerMock.VerifyAll();
        }

        [Test]
        public async Task RemoveStudentFromCourse_DeletesLink_ReturnsResult()
        {
            _dbManagerMock
                .Setup(db => db.DeleteAsync(
                    "CourseStudentLink",
                    new KeyValuePair<string, object>("CourseId", 10),
                    new KeyValuePair<string, object>("StudentId", 5)))
                .ReturnsAsync(true);
            var result = await _courseProvider.RemoveStudentFromCourse(10, 5);
            Assert.That(result.IsSuccess, Is.True);
            _dbManagerMock.VerifyAll();
        }

        [Test]
        public async Task GetTeacherId_ReturnsTeacherId()
        {
            _dbManagerMock
                .Setup(db => db.ReadObjectOfTypeAsync(
                    "SELECT TeacherId FROM Courses WHERE Id = @Id",
                    It.IsAny<Func<object[], int>>(),
                    new KeyValuePair<string, object>("Id", 10)))
                .ReturnsAsync(5);
            var result = await _courseProvider.GetTeacherId(10);
            Assert.That(result, Is.EqualTo(5));
            _dbManagerMock.VerifyAll();
        }
    }
}
