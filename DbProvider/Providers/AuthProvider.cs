using DbProvider.Database;
using DbProvider.Models;

namespace DbProvider.Providers;

public class AuthProvider : IAuthProvider
{
    private readonly IDbManager _manager;
    public AuthProvider(IDbManager manager)
    {
        _manager = manager;
    }

    public async Task<User?> AuthenticateAsync(string username, string password)
    {
        string query = "SELECT * FROM Users WHERE Username = @Username AND Password = @Password";
        string hashedPassword = HashPassword(password);
        User? user = await _manager.ReadObjectOfTypeAsync(query, ConvertUser,
            new KeyValuePair<string, object>("Username", username),
            new KeyValuePair<string, object>("Password", hashedPassword));
        return user;
    }

    private string HashPassword(string password)
    {
        using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
        {
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(password);
            byte[] hashBytes = md5.ComputeHash(inputBytes);

            return Convert.ToHexString(hashBytes);}
    }
    
    private User ConvertUser(object[] values)
    {
        int id = (int)values[0];
        string username = (string)values[1];
        string password = (string)values[2];
        string email = (string)values[3];
        short role = (short)values[4];
        return new User(id, username, password, email, role);
    }
}