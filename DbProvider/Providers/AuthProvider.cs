using System.Text.RegularExpressions;
using DbProvider.Database;
using DbProvider.Models;
using DbProvider.Models.ProviderOutputs;

namespace DbProvider.Providers;

public class AuthProvider : IAuthProvider
{
    private readonly IDbManager _manager;
    private readonly IUserProvider _userProvider;
    public AuthProvider(IDbManager manager, IUserProvider userProvider)
    {
        _manager = manager;
        _userProvider = userProvider;
    }

    public async Task<AuthResponse> AuthenticateAsync(string email, string password)
    {
        string hashedPassword = HashPassword(password);
        User? user = await _userProvider.getUserByEmailAsync(email);
        
       
        if(user == null)
            return new AuthResponse("User does not exist",false);
        if(!user.IsVerified)
            return new AuthResponse("Email is not confirmed",false);
        if(hashedPassword!= user.Password)
            return new AuthResponse("Invalid email or password",false);
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
        
        if(!res)
            return new RegisterResponse("Failed to insert token!");

        bool res2 = await _manager.InsertAsync("UserProfiles", new KeyValuePair<string, object>("UserId", userId));
        
        if(!res2)
            return new RegisterResponse("Failed to create user profile!");

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

    public async Task<ForgotPasswordResponse> ForgotPasswordAsync(string email)
    {
        User? user = await _userProvider.getUserByEmailAsync(email);
        
        if(user == null)
            return new ForgotPasswordResponse("Email not found!");
        
        Guid guid = Guid.NewGuid();
        
        bool res = await _manager.InsertAsync("VerifyTokens", new KeyValuePair<string, object>("UserId", user.Id),
            new KeyValuePair<string, object>("Token", guid.ToString()),
            new KeyValuePair<string, object>("Type", 1));

        return new ForgotPasswordResponse(guid.ToString(), res);
    }

    public async Task<BaseResponse> ChangePasswordAsync(string token, string password, string confirmPassword)
    {
        if(!ValidatePassword(password, confirmPassword))
            return "Passwords don't match!";
        
        string query = "SELECT * FROM VerifyTokens WHERE Token = @Token";
        VerifyToken? vToken = await _manager.ReadObjectOfTypeAsync(query, ConvertVerifyToken,new KeyValuePair<string, object>("Token",token));
        
        if(vToken == null)
            return "Failed to verify token!";
        
        string hashedPassword = HashPassword(password);
        
        bool res = await _manager.UpdateAsync("Users", new KeyValuePair<string, object>("Id", vToken.UserId), new KeyValuePair<string, object>("Password",hashedPassword));

        if (!res)
        {
            return "Failed to update password!";
        }
        
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

        User? user = await _userProvider.getUserByEmailAsync(email);
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
    
    
}