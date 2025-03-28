namespace DbProvider.Models.ProviderOutputs;

public class AuthResponse
{
    public string Message { get; set; }
    
    public bool IsSuccess { get; set; }
    
    public User? User { get; set; }



    public AuthResponse(User user)
    {
        IsSuccess = true;
        User = user;
    }
    
    public AuthResponse(string message, bool isSuccess)
    {
        Message = message;
        IsSuccess = isSuccess;
        User = null;
    }
}