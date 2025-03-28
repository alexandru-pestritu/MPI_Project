using System.Text.RegularExpressions;
using DbProvider.Database;
using DbProvider.Models;
using DbProvider.Models.ProviderOutputs;

namespace DbProvider.Providers;

public class AuthProvider : IAuthProvider
{
    private readonly IDbManager _manager;
    public AuthProvider(IDbManager manager)
    {
        _manager = manager;
    }

    public async Task<AuthResponse> AuthenticateAsync(string email, string password)
    {
        string query = "SELECT * FROM Users WHERE Email = @Email AND Password = @Password";
        string hashedPassword = HashPassword(password);
        User? user = await _manager.ReadObjectOfTypeAsync(query, ConvertUser,
            new KeyValuePair<string, object>("Email", email),
            new KeyValuePair<string, object>("Password", hashedPassword));
        if(user == null)
            return new AuthResponse("Invalid email or password",false);
        if(!user.IsVerified)
            return new AuthResponse("Email is not confirmed",false);
        return new AuthResponse(user);
    }

    public async Task<RegisterResponse> RegisterAsync(string username, string email, string password,string confirmPassword, short role)
    {
        if(!ValidatePassword(password, confirmPassword))
            return "Passwords do not match!";
        
        if(!ValidateEmailStructure(email))
            return "Invalid email address!";
        
        if(! await ValidateEmailAvailability(email))
            return "Email address already in use!";
        
        string hashedPassword = HashPassword(password);
        return await _manager.InsertAsync("Users", new KeyValuePair<string, object>("Username",username),
            new KeyValuePair<string, object>("Email",email),
            new KeyValuePair<string, object>("Password",hashedPassword),
            new KeyValuePair<string, object>("Role",role));
    }


    private bool ValidateEmailStructure(string email)
    {
        Regex regex = new Regex(@"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$");
        Match match = regex.Match(email);
        return match.Success;
    }

    private async Task<bool> ValidateEmailAvailability(string email)
    {
        string query = "SELECT * FROM Users WHERE Email = @Email";
        User?  user = await _manager.ReadObjectOfTypeAsync(query, ConvertUser,new KeyValuePair<string, object>("Email", email));
        return user == null;
    }

    private bool ValidatePassword(string password, string confirmPassword)
    {
        return password.Equals(confirmPassword);
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
        bool isVerified = (bool)values[5];
        return new User(id, username, password, email, role, isVerified);
    }
}