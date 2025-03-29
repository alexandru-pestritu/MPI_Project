namespace DbProvider.Models.ProviderOutputs;

public class ForgotPasswordResponse
{
    public string Token { get; set; }
    public bool IsSuccess { get; set; }
    public string Message { get; set; }

    public ForgotPasswordResponse(string token, bool isSuccess)
    {
        Token = token;
        IsSuccess = isSuccess;
    }

    public ForgotPasswordResponse(string message)
    {
        Message = message;
        IsSuccess = false;
    }
}