namespace DbProvider.Models.ProviderOutputs;

public class RegisterResponse
{
    public string Message { get; set; }
    
    public bool IsSuccess { get; set; }


   
    public static implicit operator RegisterResponse (bool isSuccess) => new RegisterResponse(){IsSuccess = isSuccess};
    public static implicit operator RegisterResponse (string message) => new RegisterResponse(){Message = message, IsSuccess = false};
}