using System;
using System.Threading.Tasks;
using DbProvider.Database;
using DbProvider.Models;
using DbProvider.Models.ProviderOutputs;
using DbProvider.Providers;
using Moq;
using NUnit.Framework;
namespace DbProvider.Tests;

    [TestFixture]
    public class AuthProviderTests
    {
        private Mock<IDbManager> _dbManagerMock;
        private Mock<IUserProvider> _userProviderMock;
        private AuthProvider _authProvider;

        [SetUp]
        public void SetUp()
        {
            _dbManagerMock = new Mock<IDbManager>();
            _userProviderMock = new Mock<IUserProvider>();
            
            _authProvider = new AuthProvider(_dbManagerMock.Object, _userProviderMock.Object);
        }

        #region AuthenticateAsync Tests

        [Test]
        public async Task AuthenticateAsync_UserNotFound_ReturnsFailureResponse()
        {
            string email = "test@example.com";
            string password = "password123";
            
            _userProviderMock
                .Setup(up => up.getUserByEmailAsync(email))
                .ReturnsAsync((User?)null);
            
            var result = await _authProvider.AuthenticateAsync(email, password);
            
            Assert.That(result.IsSuccess, Is.False, "Expected IsSuccess to be false when user is not found.");
            Assert.That(result.Message, Is.EqualTo("User does not exist"));
            _userProviderMock.Verify(up => up.getUserByEmailAsync(email), Times.Once);
        }

        [Test]
        public async Task AuthenticateAsync_UserNotVerified_ReturnsFailureResponse()
        {
            string email = "test@example.com";
            string password = "password123";
            
            var user = new User(
                id: 1,
                username: "testUser",
                password: _testHelper_HashPassword("password123"),
                email: email,
                role: 1,
                isVerified: false
            );
            
            _userProviderMock
                .Setup(up => up.getUserByEmailAsync(email))
                .ReturnsAsync(user);
            
            var result = await _authProvider.AuthenticateAsync(email, password);
            
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Message, Is.EqualTo("Email is not confirmed"));
        }

        [Test]
        public async Task AuthenticateAsync_InvalidPassword_ReturnsFailureResponse()
        {
            string email = "test@example.com";
            string correctPassword = "password123";
            string incorrectPassword = "wrongPassword";
            
            var user = new User(
                id: 1,
                username: "testUser",
                password: _testHelper_HashPassword(correctPassword),
                email: email,
                role: 1,
                isVerified: true
            );

            _userProviderMock
                .Setup(up => up.getUserByEmailAsync(email))
                .ReturnsAsync(user);
            
            var result = await _authProvider.AuthenticateAsync(email, incorrectPassword);
            
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Message, Is.EqualTo("Invalid email or password"));
        }

        [Test]
        public async Task AuthenticateAsync_ValidCredentials_ReturnsSuccessResponse()
        {
            string email = "test@example.com";
            string password = "password123";

            var user = new User(
                id: 1,
                username: "testUser",
                password: _testHelper_HashPassword(password),
                email: email,
                role: 1,
                isVerified: true
            );

            _userProviderMock
                .Setup(up => up.getUserByEmailAsync(email))
                .ReturnsAsync(user);
            
            var result = await _authProvider.AuthenticateAsync(email, password);
            
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.User, Is.Not.Null);
            Assert.That(result.User.Email, Is.EqualTo(email));
        }

        #endregion

        #region RegisterAsync Tests

        [Test]
        public async Task RegisterAsync_PasswordsDoNotMatch_ReturnsFailureResponse()
        {
            string username = "testUser";
            string email = "test@example.com";
            string password = "password123";
            string confirmPassword = "mismatch";
            short role = 1;
            
            var result = await _authProvider.RegisterAsync(username, email, password, confirmPassword, role);
            
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Message, Is.EqualTo("Passwords don't match!"));
        }

        [Test]
        public async Task RegisterAsync_InvalidEmailFormat_ReturnsFailureResponse()
        {
            string username = "testUser";
            string email = "notAnEmail";
            string password = "password123";
            short role = 1;
            
            var result = await _authProvider.RegisterAsync(username, email, password, password, role);
            
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Message, Is.EqualTo("Invalid email address!"));
        }

        [Test]
        public async Task RegisterAsync_EmailAlreadyUsed_ReturnsFailureResponse()
        {
            string username = "testUser";
            string email = "test@example.com";
            string password = "password123";
            short role = 1;
            
            _userProviderMock
                .Setup(up => up.getUserByEmailAsync(email))
                .ReturnsAsync(new User(
                    id: 1,
                    username: "alreadyTaken",
                    password: "SomeHash",
                    email: email,
                    role: 1,
                    isVerified: true
                ));
            
            var result = await _authProvider.RegisterAsync(username, email, password, password, role);
            
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Message, Is.EqualTo("Email address already in use!"));
        }

        [Test]
        public async Task RegisterAsync_InsertUserFails_ReturnsFailureResponse()
        {
            string username = "testUser";
            string email = "test@example.com";
            string password = "password123";
            short role = 1;
            
            _userProviderMock
                .Setup(up => up.getUserByEmailAsync(email))
                .ReturnsAsync((User?)null);
            
            _dbManagerMock
                .Setup(db => db.InsertAsyncWithReturn<int>(
                    "Users", "Id",
                    It.IsAny<KeyValuePair<string, object>[]>()))
                .ReturnsAsync(-1);
            
            var result = await _authProvider.RegisterAsync(username, email, password, password, role);
            
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Message, Is.EqualTo("Failed to register user!"));
        }

        [Test]
        public async Task RegisterAsync_SuccessfulRegistration_ReturnsSuccessResponse()
        {
            string username = "testUser";
            string email = "test@example.com";
            string password = "password123";
            short role = 1;
            
            _userProviderMock
                .Setup(up => up.getUserByEmailAsync(email))
                .ReturnsAsync((User?)null);
            
            _dbManagerMock
                .Setup(db => db.InsertAsyncWithReturn<int>(
                    "Users", "Id",
                    It.IsAny<KeyValuePair<string, object>[]>()))
                .ReturnsAsync(42);
            
            _dbManagerMock
                .Setup(db => db.InsertAsync(
                    "VerifyTokens",
                    It.IsAny<KeyValuePair<string, object>[]>()))
                .ReturnsAsync(true);
            
            _dbManagerMock
                .Setup(db => db.InsertAsync(
                    "UserProfiles",
                    It.IsAny<KeyValuePair<string, object>[]>()))
                .ReturnsAsync(true);
            
            var result = await _authProvider.RegisterAsync(username, email, password, password, role);
            
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Token, Is.Not.Null.And.Not.Empty);
        }

        #endregion

        #region VerifyUserAsync Tests

        [Test]
        public async Task VerifyUserAsync_TokenNotFound_ReturnsFailure()
        {
            string token = "testToken";
            
            _dbManagerMock
                .Setup(db => db.ReadObjectOfTypeAsync(
                    It.IsAny<string>(),
                    It.IsAny<Func<object[], VerifyToken>>(),
                    It.IsAny<KeyValuePair<string, object>[]>()))
                .ReturnsAsync((VerifyToken?)null);
            
            var result = await _authProvider.VerifyUserAsync(token);
            
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Message, Is.EqualTo("Failed to verify token!"));
        }

        [Test]
        public async Task VerifyUserAsync_TokenFound_UpdatesUserAndDeletesToken()
        {
            string token = "testToken";
            var verifyToken = new VerifyToken(1, 2, token, 0);
            
            _dbManagerMock
                .Setup(db => db.ReadObjectOfTypeAsync(
                    It.IsAny<string>(),
                    It.IsAny<Func<object[], VerifyToken>>(),
                    It.IsAny<KeyValuePair<string, object>[]>()))
                .ReturnsAsync(verifyToken);
            
            _dbManagerMock
                .Setup(db => db.UpdateAsync(
                    "Users",
                    It.IsAny<KeyValuePair<string, object>>(),
                    It.IsAny<KeyValuePair<string, object>>()))
                .ReturnsAsync(true);

            _dbManagerMock
                .Setup(db => db.DeleteAsync(
                    "VerifyTokens",
                    It.IsAny<KeyValuePair<string, object>>()))
                .ReturnsAsync(true);

            var result = await _authProvider.VerifyUserAsync(token);

            Assert.That(result.IsSuccess, Is.True);
        }

        #endregion

        #region ForgotPasswordAsync Tests

        [Test]
        public async Task ForgotPasswordAsync_EmailNotFound_ReturnsFailure()
        {
            string email = "nonexistent@example.com";

            _userProviderMock
                .Setup(up => up.getUserByEmailAsync(email))
                .ReturnsAsync((User?)null);

            var result = await _authProvider.ForgotPasswordAsync(email);

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Message, Is.EqualTo("Email not found!"));
        }

        [Test]
        public async Task ForgotPasswordAsync_SuccessfulTokenCreation_ReturnsSuccess()
        {
            string email = "test@example.com";
            var user = new User(
                id: 5,
                username: "testUser",
                password: "SomeHash",
                email: email,
                role: 1,
                isVerified: true
            );

            _userProviderMock
                .Setup(up => up.getUserByEmailAsync(email))
                .ReturnsAsync(user);

            _dbManagerMock
                .Setup(db => db.InsertAsync(
                    "VerifyTokens",
                    It.IsAny<KeyValuePair<string, object>[]>()))
                .ReturnsAsync(true);

            var result = await _authProvider.ForgotPasswordAsync(email);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Token, Is.Not.Null.And.Not.Empty);
        }

        #endregion

        #region ChangePasswordAsync Tests

        [Test]
        public async Task ChangePasswordAsync_PasswordsDoNotMatch_ReturnsFailure()
        {
            string token = "testToken";
            string newPassword = "newPass123";
            string confirmPassword = "mismatch";

            var result = await _authProvider.ChangePasswordAsync(token, newPassword, confirmPassword);

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Message, Is.EqualTo("Passwords don't match!"));
        }

        [Test]
        public async Task ChangePasswordAsync_TokenNotFound_ReturnsFailure()
        {
            string token = "testToken";
            string newPassword = "newPass123";

            _dbManagerMock
                .Setup(db => db.ReadObjectOfTypeAsync(
                    It.IsAny<string>(),
                    It.IsAny<Func<object[], VerifyToken>>(),
                    It.IsAny<KeyValuePair<string, object>[]>()))
                .ReturnsAsync((VerifyToken?)null);

            var result = await _authProvider.ChangePasswordAsync(token, newPassword, newPassword);

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Message, Is.EqualTo("Failed to verify token!"));
        }

        [Test]
        public async Task ChangePasswordAsync_SuccessfulPasswordChange_DeletesToken()
        {
            string token = "testToken";
            string newPassword = "newPass123";
            var verifyToken = new VerifyToken(1, 2, token, 1);

            _dbManagerMock
                .Setup(db => db.ReadObjectOfTypeAsync(
                    It.IsAny<string>(),
                    It.IsAny<Func<object[], VerifyToken>>(),
                    It.IsAny<KeyValuePair<string, object>[]>()))
                .ReturnsAsync(verifyToken);

            _dbManagerMock
                .Setup(db => db.UpdateAsync(
                    "Users",
                    It.IsAny<KeyValuePair<string, object>>(),
                    It.IsAny<KeyValuePair<string, object>>()))
                .ReturnsAsync(true);

            _dbManagerMock
                .Setup(db => db.DeleteAsync(
                    "VerifyTokens",
                    It.IsAny<KeyValuePair<string, object>>()))
                .ReturnsAsync(true);

            var result = await _authProvider.ChangePasswordAsync(token, newPassword, newPassword);

            Assert.That(result.IsSuccess, Is.True);
        }

        #endregion

        #region Private Helper

        /// <summary>
        /// Reproduce the same MD5 hash logic used in AuthProvider so we can match the hashed password in tests.
        /// </summary>
        private string _testHelper_HashPassword(string password)
        {
            using var md5 = System.Security.Cryptography.MD5.Create();
            var inputBytes = System.Text.Encoding.ASCII.GetBytes(password);
            var hashBytes = md5.ComputeHash(inputBytes);
            return Convert.ToHexString(hashBytes);
        }

        #endregion
    }
