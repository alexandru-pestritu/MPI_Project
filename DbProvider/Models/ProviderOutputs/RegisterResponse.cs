namespace DbProvider.Models.ProviderOutputs;

public class RegisterResponse
{
    public string Token { get; set; }
    public bool IsSuccess { get; set; }
    public string Message { get; set; }

    public RegisterResponse(string token, bool isSuccess)
    {
        Token = token;
        IsSuccess = isSuccess;
    }

    public RegisterResponse(string message)
    {
        Message = message;
        IsSuccess = false;
    }
}