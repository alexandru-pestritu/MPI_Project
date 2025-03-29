namespace backend.Models;

public class ResetPasswordRequest
{
    public string Token { get; set; }
    public string Password { get; set; }
    public string ConfirmPassword { get; set; }
    
    public ResetPasswordRequest(string token, string password, string confirmPassword)
    {
        Token = token;
        Password = password;
        ConfirmPassword = confirmPassword;
    }
}