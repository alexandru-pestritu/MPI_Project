namespace DbProvider.Models.ProviderOutputs;

public class BaseResponse
{
    public string Message { get; set; }
    
    public bool IsSuccess { get; set; }


   
    public static implicit operator BaseResponse (bool isSuccess) => new BaseResponse(){IsSuccess = isSuccess};
    public static implicit operator BaseResponse (string message) => new BaseResponse(){Message = message, IsSuccess = false};
}