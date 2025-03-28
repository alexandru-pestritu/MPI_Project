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
            return new RegisterResponse("Passwords don't match!");
        
        if(!ValidateEmailStructure(email))
            return new RegisterResponse("Invalid email address!");
        
        if(! await ValidateEmailAvailability(email))
            return new RegisterResponse("Email address already in use!");
        
        string hashedPassword = HashPassword(password);
        
        
        
        int? result = await _manager.InsertAsyncWithReturn<int>("Users","Id", new KeyValuePair<string, object>("Username",username),
            new KeyValuePair<string, object>("Email",email),
            new KeyValuePair<string, object>("Password",hashedPassword),
            new KeyValuePair<string, object>("Role",role));

        int userId = result.GetValueOrDefault(-1);
        
        if(userId == -1)
            return new RegisterResponse("Failed to register user!");
        
        Guid guid = Guid.NewGuid();

        bool res = await _manager.InsertAsync("VerifyTokens", new KeyValuePair<string, object>("UserId", userId),
            new KeyValuePair<string, object>("Token", guid.ToString()),
            new KeyValuePair<string, object>("Type", 0));

        return new RegisterResponse(guid.ToString(), res);
    }

    public async Task<BaseResponse> VerifyUserAsync(string token)
    {
        string query = "SELECT * FROM VerifyTokens WHERE Token = @Token";
        VerifyToken? vToken = await _manager.ReadObjectOfTypeAsync(query,ConvertVerifyToken,new KeyValuePair<string, object>("Token",token));
        
        if(vToken == null)
            return "Failed to verify token!";
        
        await _manager.UpdateAsync("Users", new KeyValuePair<string, object>("Id", vToken.UserId), new KeyValuePair<string, object>("IsVerified",true));
        
        return await _manager.DeleteAsync("VerifyTokens", new KeyValuePair<string, object>("Token", token));
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

    private VerifyToken ConvertVerifyToken(object[] values)
    {
        int id = (int)values[0];
        int userId = (int)values[1];
        string token = (string)values[2];
        int type = (int)values[3];
        
        return new VerifyToken(id,userId,token,type);
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