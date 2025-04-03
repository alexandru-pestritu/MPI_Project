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
    public class UserProviderTests
    {
        private Mock<IDbManager> _dbManagerMock;
        private UserProvider _userProvider;

        [SetUp]
        public void SetUp()
        {
            _dbManagerMock = new Mock<IDbManager>(MockBehavior.Strict);
            _userProvider = new UserProvider(_dbManagerMock.Object);
        }

        #region getUserProfileAsync Tests

        [Test]
        public async Task getUserProfileAsync_NoProfileFound_ReturnsNull()
        {
            int userId = 123;
            _dbManagerMock
                .Setup(db => db.ReadObjectOfTypeAsync(
                    It.IsAny<string>(),
                    It.IsAny<Func<object[], UserProfile>>(),
                    It.Is<KeyValuePair<string, object>[]>(p => (int)p[0].Value == userId)))
                .ReturnsAsync((UserProfile?)null);
            
            var result = await _userProvider.getUserProfileAsync(userId);
            
            Assert.That(result, Is.Null);
            _dbManagerMock.VerifyAll();
        }

        [Test]
        public async Task getUserProfileAsync_ProfileExists_ReturnsProfile()
        {
            int userId = 123;
            var mockProfile = new UserProfile(
                id: 10,
                userId: userId,
                firstName: "John",
                lastName: "Doe",
                bio: "Sample Bio"
            );

            _dbManagerMock
                .Setup(db => db.ReadObjectOfTypeAsync(
                    It.IsAny<string>(),
                    It.IsAny<Func<object[], UserProfile>>(),
                    It.Is<KeyValuePair<string, object>[]>(p => (int)p[0].Value == userId)))
                .ReturnsAsync(mockProfile);

            var result = await _userProvider.getUserProfileAsync(userId);

            Assert.That(result, Is.Not.Null);
            Assert.That(result?.Id, Is.EqualTo(mockProfile.Id));
            Assert.That(result?.UserId, Is.EqualTo(mockProfile.UserId));
            Assert.That(result?.FirstName, Is.EqualTo(mockProfile.FirstName));
            _dbManagerMock.VerifyAll();
        }

        #endregion

        #region updateUserProfileAsync Tests

        [Test]
        public async Task updateUserProfileAsync_UpdateFails_ReturnsNull()
        {
            var profileToUpdate = new UserProfile(1, 100, "Jane", "Doe", "Bio here");

            _dbManagerMock
                .Setup(db => db.UpdateAsync(
                    "UserProfiles",
                    It.Is<KeyValuePair<string, object>>(k => k.Key == "UserId" && (int)k.Value == profileToUpdate.UserId),
                    It.IsAny<KeyValuePair<string, object>>(),
                    It.IsAny<KeyValuePair<string, object>>(),
                    It.IsAny<KeyValuePair<string, object>>()))
                .ReturnsAsync(false);


            var result = await _userProvider.updateUserProfileAsync(profileToUpdate);

            Assert.That(result, Is.Null);
            _dbManagerMock.VerifyAll();
        }

        [Test]
        public async Task updateUserProfileAsync_UpdateSucceeds_ReturnsUpdatedProfile()
        {
            var profileToUpdate = new UserProfile(1, 100, "Jane", "Doe", "Bio here");

            _dbManagerMock
                .Setup(db => db.UpdateAsync(
                    "UserProfiles",
                    It.Is<KeyValuePair<string, object>>(k => (string)k.Key == "UserId" && (int)k.Value == profileToUpdate.UserId),
                    It.IsAny<KeyValuePair<string, object>>(),
                    It.IsAny<KeyValuePair<string, object>>(),
                    It.IsAny<KeyValuePair<string, object>>()))
                .ReturnsAsync(true);

            _dbManagerMock
                .Setup(db => db.ReadObjectOfTypeAsync(
                    It.IsAny<string>(),
                    It.IsAny<Func<object[], UserProfile>>(),
                    It.IsAny<KeyValuePair<string, object>[]>()))
                .ReturnsAsync(profileToUpdate);

            var result = await _userProvider.updateUserProfileAsync(profileToUpdate);

            Assert.That(result, Is.Not.Null);
            Assert.That(result?.UserId, Is.EqualTo(profileToUpdate.UserId));
            Assert.That(result?.FirstName, Is.EqualTo("Jane"));
            _dbManagerMock.VerifyAll();
        }

        #endregion

        #region getUserByIdAsync Tests

        [Test]
        public async Task getUserByIdAsync_NoUserFound_ReturnsNull()
        {
            int userId = 999;
            _dbManagerMock
                .Setup(db => db.ReadObjectOfTypeAsync(
                    It.IsAny<string>(),
                    It.IsAny<Func<object[], User>>(),
                    It.Is<KeyValuePair<string, object>[]>(p => (int)p[0].Value == userId)))
                .ReturnsAsync((User?)null);

            var result = await _userProvider.getUserByIdAsync(userId);

            Assert.That(result, Is.Null);
            _dbManagerMock.VerifyAll();
        }

        [Test]
        public async Task getUserByIdAsync_UserFound_ReturnsUser()
        {
            int userId = 101;
            var mockUser = new User(
                id: userId,
                username: "MockUser",
                password: "pass",
                email: "user@example.com",
                role: 1,
                isVerified: true
            );

            _dbManagerMock
                .Setup(db => db.ReadObjectOfTypeAsync(
                    It.IsAny<string>(),
                    It.IsAny<Func<object[], User>>(),
                    It.Is<KeyValuePair<string, object>[]>(p => (int)p[0].Value == userId)))
                .ReturnsAsync(mockUser);

            var result = await _userProvider.getUserByIdAsync(userId);

            Assert.That(result, Is.Not.Null);
            Assert.That(result?.Id, Is.EqualTo(userId));
            Assert.That(result?.Username, Is.EqualTo("MockUser"));
            _dbManagerMock.VerifyAll();
        }

        #endregion

        #region getUserByEmailAsync Tests

        [Test]
        public async Task getUserByEmailAsync_NoUserFound_ReturnsNull()
        {
            string email = "nonexistent@example.com";
            _dbManagerMock
                .Setup(db => db.ReadObjectOfTypeAsync(
                    It.IsAny<string>(),
                    It.IsAny<Func<object[], User>>(),
                    It.Is<KeyValuePair<string, object>[]>(p => (string)p[0].Value == email)))
                .ReturnsAsync((User?)null);

            var result = await _userProvider.getUserByEmailAsync(email);

            Assert.That(result, Is.Null);
            _dbManagerMock.VerifyAll();
        }

        [Test]
        public async Task getUserByEmailAsync_UserFound_ReturnsUser()
        {
            string email = "test@example.com";
            var mockUser = new User(
                id: 55,
                username: "EmailUser",
                password: "someHash",
                email: email,
                role: 0,
                isVerified: false
            );

            _dbManagerMock
                .Setup(db => db.ReadObjectOfTypeAsync(
                    It.IsAny<string>(),
                    It.IsAny<Func<object[], User>>(),
                    It.Is<KeyValuePair<string, object>[]>(p => (string)p[0].Value == email)))
                .ReturnsAsync(mockUser);

            var result = await _userProvider.getUserByEmailAsync(email);

            Assert.That(result, Is.Not.Null);
            Assert.That(result?.Email, Is.EqualTo(email));
            _dbManagerMock.VerifyAll();
        }

        #endregion

        #region GetStudentsInCourse Tests

        [Test]
        public async Task GetStudentsInCourse_NoUsers_ReturnsEmptyList()
        {
            int courseId = 123;
            var emptyUserList = new List<User>();
            
            _dbManagerMock
                .Setup(db => db.ReadListOfTypeAsync(
                    It.IsAny<string>(),
                    It.IsAny<Func<object[], User>>(),
                    It.Is<KeyValuePair<string, object>[]>(p => (int)p[0].Value == courseId)))
                .ReturnsAsync(emptyUserList);

            var result = await _userProvider.GetStudentsInCourse(courseId);

            Assert.That(result, Is.Empty);
            _dbManagerMock.VerifyAll();
        }

        [Test]
        public async Task GetStudentsInCourse_UsersFound_ReturnsTheirProfiles()
        {
            int courseId = 123;
            var userList = new List<User>
            {
                new User(10, "User1", "pass1", "u1@example.com", 0, true),
                new User(11, "User2", "pass2", "u2@example.com", 0, true)
            };

            var profileForUser10 = new UserProfile(1, 10, "John", "One", "Bio1");
            var profileForUser11 = new UserProfile(2, 11, "Jane", "Two", "Bio2");

            _dbManagerMock
                .Setup(db => db.ReadListOfTypeAsync(
                    It.IsAny<string>(),
                    It.IsAny<Func<object[], User>>(),
                    It.Is<KeyValuePair<string, object>[]>(p => (int)p[0].Value == courseId)))
                .ReturnsAsync(userList);
            
            _dbManagerMock
                .SetupSequence(db => db.ReadObjectOfTypeAsync(
                    It.IsAny<string>(),
                    It.IsAny<Func<object[], UserProfile>>(),
                    It.IsAny<KeyValuePair<string, object>[]>()))
                .ReturnsAsync(profileForUser10)
                .ReturnsAsync(profileForUser11);

            var result = await _userProvider.GetStudentsInCourse(courseId);

            Assert.That(result, Has.Count.EqualTo(2));
            Assert.That(result[0].UserId, Is.EqualTo(10));
            Assert.That(result[1].UserId, Is.EqualTo(11));
            _dbManagerMock.VerifyAll();
        }

        #endregion

        #region GetAllStudents Tests

        [Test]
        public async Task GetAllStudents_NoStudents_ReturnsEmptyList()
        {

            var emptyList = new List<User>();
            _dbManagerMock
                .Setup(db => db.ReadListOfTypeAsync(
                    It.IsAny<string>(),
                    It.IsAny<Func<object[], User>>()))
                .ReturnsAsync(emptyList);

            var result = await _userProvider.GetAllStudents();

            Assert.That(result, Is.Empty);
            _dbManagerMock.VerifyAll();
        }

        [Test]
        public async Task GetAllStudents_StudentsFound_ReturnsProfiles()
        {
            var studentUsers = new List<User>
            {
                new User(101, "Student1", "hash1", "s1@example.com", 0, true),
                new User(102, "Student2", "hash2", "s2@example.com", 0, true)
            };

            var profile101 = new UserProfile(1, 101, "Stud", "One", "Bio1");
            var profile102 = new UserProfile(2, 102, "Stud", "Two", "Bio2");

            _dbManagerMock
                .Setup(db => db.ReadListOfTypeAsync(
                    It.IsAny<string>(),
                    It.IsAny<Func<object[], User>>()))
                .ReturnsAsync(studentUsers);

            _dbManagerMock
                .SetupSequence(db => db.ReadObjectOfTypeAsync(
                    It.IsAny<string>(),
                    It.IsAny<Func<object[], UserProfile>>(),
                    It.IsAny<KeyValuePair<string, object>[]>()))
                .ReturnsAsync(profile101)
                .ReturnsAsync(profile102);

            var result = await _userProvider.GetAllStudents();

            Assert.That(result, Has.Count.EqualTo(2));
            Assert.That(result[0].UserId, Is.EqualTo(101));
            Assert.That(result[1].UserId, Is.EqualTo(102));
            _dbManagerMock.VerifyAll();
        }

        #endregion
    }
}
